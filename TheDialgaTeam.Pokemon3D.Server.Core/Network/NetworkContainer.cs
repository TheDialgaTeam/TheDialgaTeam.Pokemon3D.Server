// Pokemon 3D Server Client
// Copyright (C) 2023 Yong Jian Ming
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using Mediator;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Formats;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Events;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public sealed class NetworkContainer :
    INotificationHandler<ClientConnected>,
    INotificationHandler<ClientDisconnected>,
    INotificationHandler<NewPacketReceived>,
    INotificationHandler<PlayerJoin>,
    INotificationHandler<PlayerUpdated>,
    INotificationHandler<PlayerLeft>,
    INotificationHandler<WorldUpdate>
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly IPokemonServerOptions _options;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly IPlayerFactory _playerFactory;

    private readonly ConcurrentDictionary<IPokemonServerClient, IPlayer?> _players = new();

    private int _nextRunningId = 1;
    private readonly SortedSet<int> _runningIds = [];
    private readonly object _runningIdLock = new();

    public NetworkContainer(
        ILogger<NetworkContainer> logger,
        IMediator mediator,
        IPokemonServerOptions options,
        IStringLocalizer stringLocalizer,
        IPlayerFactory playerFactory)
    {
        _logger = logger;
        _mediator = mediator;
        _options = options;
        _stringLocalizer = stringLocalizer;
        _playerFactory = playerFactory;
    }

    #region Client Event Handler

    public async ValueTask Handle(ClientConnected notification, CancellationToken cancellationToken)
    {
        try
        {
            if (!_players.TryAdd(notification.PokemonServerClient, null))
            {
                await notification.PokemonServerClient.DisconnectAsync().ConfigureAwait(false);
            }
        }
        catch
        {
            await notification.PokemonServerClient.DisconnectAsync().ConfigureAwait(false);
        }
    }

    public ValueTask Handle(ClientDisconnected notification, CancellationToken cancellationToken)
    {
        if (notification.PokemonServerClient is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (_players.TryRemove(notification.PokemonServerClient, out var player))
        {
            if (player is not null)
            {
                lock (_runningIdLock)
                {
                    _runningIds.Add(player.Id);
                }

                return _mediator.Publish(new PlayerLeft(player, notification.Reason), cancellationToken);
            }
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask Handle(NewPacketReceived notification, CancellationToken cancellationToken)
    {
        try
        {
            switch (notification.RawPacket.PacketType)
            {
                case PacketType.GameData:
                {
                    await HandleGameDataPacket(notification, cancellationToken).ConfigureAwait(false);
                    break;
                }

                case PacketType.ChatMessage:
                {
                    foreach (var player in GetPlayersEnumerable())
                    {
                        player.SendPacket(notification.RawPacket);
                    }
                    break;
                }

                case PacketType.ServerDataRequest:
                {
                    await HandleServerDataRequest(notification, cancellationToken).ConfigureAwait(false);
                    break;
                }
            }
        }
        catch (Exception exception)
        {
            await notification.Network.KickAsync(_stringLocalizer[s => s.GameMessageFormat.ServerError]).ConfigureAwait(false);
            _logger.LogError(exception, "{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.ServerError, exception.Message]);
        }
    }
    
    private async ValueTask HandleGameDataPacket(NewPacketReceived notification, CancellationToken cancellationToken)
    {
        if (GameDataPacket.IsFullGameData(notification.RawPacket))
        {
            // This is a new player joining.
            var gameDataPacket = new GameDataPacket(notification.RawPacket);

            var playerCanJoin = true;
            var reason = string.Empty;

            // Check Server Space Limit.
            var playerCount = GetPlayerCount();

            if (playerCount >= _options.ServerOptions.MaxPlayers)
            {
                playerCanJoin = false;
                reason = _stringLocalizer[s => s.GameMessageFormat.ServerIsFull];
            }

            // Check Profile Type.
            if (!_options.ServerOptions.OfflineMode && !gameDataPacket.IsGameJoltPlayer)
            {
                playerCanJoin = false;
                reason = _stringLocalizer[s => s.GameMessageFormat.ServerOnlyAllowGameJoltProfile];
            }

            switch (_options.ServerOptions.AllowAnyGameModes)
            {
                // Check GameMode
                case true when _options.ServerOptions.BlacklistedGameModes.Any(s => gameDataPacket.GameMode.Equals(s, StringComparison.OrdinalIgnoreCase)):
                {
                    playerCanJoin = false;
                    reason = _stringLocalizer[s => s.GameMessageFormat.ServerBlacklistedGameModes];
                    break;
                }

                case false when !_options.ServerOptions.WhitelistedGameModes.Any(s => gameDataPacket.GameMode.Equals(s, StringComparison.OrdinalIgnoreCase)):
                {
                    playerCanJoin = false;
                    reason = _stringLocalizer[s => s.GameMessageFormat.ServerWhitelistedGameModes, new ArrayFormat<string>(_options.ServerOptions.WhitelistedGameModes)];
                    break;
                }
            }

            if (!playerCanJoin)
            {
                await notification.Network.KickAsync(reason).ConfigureAwait(false);
                _logger.LogInformation("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.PlayerUnableToJoin, new PlayerNameFormat(_stringLocalizer, gameDataPacket), reason]);
                return;
            }

            // If all okay, create a new player object.
            var player = _playerFactory.CreatePlayer(notification.Network, GetNextRunningId(), gameDataPacket);
            _players[notification.Network] = player;
            
            await _mediator.Publish(new PlayerJoin(player), cancellationToken).ConfigureAwait(false);
        }
        else
        {
            // This is updating existing player.
            await GetPlayerById(notification.RawPacket.Origin).ApplyGameDataAsync(notification.RawPacket).ConfigureAwait(false);
        }
    }
    
    private ValueTask HandleServerDataRequest(NewPacketReceived notification, CancellationToken cancellationToken)
    {
        notification.Network.SendPacket(new ServerInfoDataPacket(
            GetPlayerCount(),
            _options.ServerOptions.MaxPlayers,
            _options.ServerOptions.ServerName,
            _options.ServerOptions.ServerDescription,
            GetPlayerDisplayNames()).ToRawPacket());
        
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Player Event Handler

    public async ValueTask Handle(PlayerJoin notification, CancellationToken cancellationToken)
    {
        await notification.Player.InitializePlayer(cancellationToken).ConfigureAwait(false);
        
        notification.Player.SendPacket(new IdPacket(notification.Player.Id).ToRawPacket());

        foreach (var player in GetPlayersEnumerable(null, true))
        {
            if (player != notification.Player)
            {
                notification.Player.SendPacket(new CreatePlayerPacket(player.Id).ToRawPacket());
                notification.Player.SendPacket(player.ToGameDataPacket().ToRawPacket());
            }
            
            player.SendPacket(new CreatePlayerPacket(notification.Player.Id).ToRawPacket());
            player.SendPacket(notification.Player.ToGameDataPacket().ToRawPacket());
            player.SendPacket(new ChatMessagePacket(-1, _stringLocalizer[s => s.GameMessageFormat.PlayerJoin, new PlayerNameFormat(_stringLocalizer, notification.Player)]).ToRawPacket());
        }

        foreach (var welcomeMessage in _options.ServerOptions.WelcomeMessage)
        {
            notification.Player.SendPacket(new ChatMessagePacket(-1, welcomeMessage).ToRawPacket());
        }
        
        _logger.LogInformation("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.PlayerJoin, new PlayerNameFormat(_stringLocalizer, notification.Player)]);
    }

    public ValueTask Handle(PlayerUpdated notification, CancellationToken cancellationToken)
    {
        foreach (var player in GetPlayersEnumerable(null, true))
        {
            player.SendPacket(notification.Player.ToGameDataPacket().ToRawPacket());
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(PlayerLeft notification, CancellationToken cancellationToken)
    {
        foreach (var player in GetPlayersEnumerable(null, true))
        {
            player.SendPacket(new DestroyPlayerPacket(notification.Player.Id).ToRawPacket());
            player.SendPacket(new ChatMessagePacket(-1, _stringLocalizer[s => s.GameMessageFormat.PlayerLeft, new PlayerNameFormat(_stringLocalizer, notification.Player)]).ToRawPacket());
        }

        if (notification.Player is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _logger.LogInformation("{Message}", notification.Reason is null ? _stringLocalizer[s => s.ConsoleMessageFormat.PlayerLeft, new PlayerNameFormat(_stringLocalizer, notification.Player)] : _stringLocalizer[s => s.ConsoleMessageFormat.PlayerLeftWithReason, new PlayerNameFormat(_stringLocalizer, notification.Player), notification.Reason]);

        return ValueTask.CompletedTask;
    }

    #endregion

    #region Player Related Functions

    private int GetNextRunningId()
    {
        lock (_runningIdLock)
        {
            if (_runningIds.Count <= 0) return _nextRunningId++;

            var id = _runningIds.Min;
            _runningIds.Remove(id);
            return id;
        }
    }
    
    private int GetPlayerCount()
    {
        return _players.Values.Count(player => player is not null);
    }

    private IPlayer GetPlayerById(int id)
    {
        return _players.Values.Single(player => player is not null && player.Id == id)!;
    }
    
    private string[] GetPlayerDisplayNames()
    {
        var playerCount = GetPlayerCount();

        if (playerCount > 20)
        {
            return _players.Values
                .Where(player => player is not null)
                .OrderBy(player => player!.Name)
                .Take(20)
                .Select(player => player!.DisplayName)
                .Append($"(And {playerCount - 20} more)")
                .ToArray();
        }

        return _players.Values
            .Where(player => player is not null)
            .OrderBy(player => player!.Name)
            .Select(player => player!.DisplayName)
            .ToArray();
    }

    private IEnumerable<IPlayer> GetPlayersEnumerable(IPlayer? excludePlayer = null, bool includeNonReady = false)
    {
        return (includeNonReady ? _players.Values.Where(player => player is not null && player != excludePlayer) : _players.Values.Where(player => player is not null && player.IsReady && player != excludePlayer))!;
    }

    #endregion

    public ValueTask Handle(WorldUpdate notification, CancellationToken cancellationToken)
    {
        if (notification.World.IsGlobalWorld)
        {
            foreach (var player in _players.Values.Where(player => player is not null && player.IsReady))
            {
                player!.SendPacket(notification.World.GetWorldDataPacket().ToRawPacket());
            }
        }
        
        return ValueTask.CompletedTask;
    }
}