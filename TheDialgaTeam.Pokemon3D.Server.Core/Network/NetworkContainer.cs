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
using TheDialgaTeam.Pokemon3D.Server.Core.World.Commands;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public sealed class NetworkContainer :
    INotificationHandler<ClientConnected>,
    INotificationHandler<ClientDisconnected>,
    INotificationHandler<NewPacketReceived>,
    INotificationHandler<PlayerJoin>,
    INotificationHandler<PlayerUpdated>,
    INotificationHandler<PlayerLeft>,
    ICommandHandler<StartGlobalWorld>,
    ICommandHandler<StopGlobalWorld>
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly IPokemonServerOptions _options;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly IPlayerFactory _playerFactory;

    private readonly ConcurrentDictionary<IPokemonServerClient, IPlayer?> _players = new();
    private readonly ILocalWorld _globalWorld;

    private int _nextRunningId = 1;
    private readonly SortedSet<int> _runningIds = [];
    private readonly object _runningIdLock = new();

    public NetworkContainer(
        ILogger<NetworkContainer> logger,
        IMediator mediator,
        IPokemonServerOptions options,
        IStringLocalizer stringLocalizer,
        IPlayerFactory playerFactory,
        ILocalWorldFactory worldFactory)
    {
        _logger = logger;
        _mediator = mediator;
        _options = options;
        _stringLocalizer = stringLocalizer;
        _playerFactory = playerFactory;
        _globalWorld = worldFactory.CreateLocalWorld();
    }

    #region Client Events

    public ValueTask Handle(ClientConnected notification, CancellationToken cancellationToken)
    {
        _players.TryAdd(notification.PokemonServerClient, null);
        return ValueTask.CompletedTask;
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

    #endregion

    #region Packet Handler

    public ValueTask Handle(NewPacketReceived notification, CancellationToken cancellationToken)
    {
        return notification.RawPacket.PacketType switch
        {
            PacketType.GameData => HandleGameDataPacket(notification, cancellationToken),
            PacketType.ChatMessage => HandleChatMessage(notification, cancellationToken),
            PacketType.GamestateMessage => HandleGamestateMessage(notification, cancellationToken),
            PacketType.ServerDataRequest => HandleServerDataRequest(notification, cancellationToken),
            var _ => ValueTask.CompletedTask
        };
    }

    private ValueTask HandleGameDataPacket(NewPacketReceived notification, CancellationToken cancellationToken)
    {
        if (!GameDataPacket.IsFullGameData(notification.RawPacket))
        {
            return GetPlayerById(notification.RawPacket.Origin).ApplyGameDataAsync(notification.RawPacket);
        }

        // This is a new player joining.
        var gameDataPacket = new GameDataPacket(notification.RawPacket);

        var player = _playerFactory.CreatePlayer(notification.Network, GetNextRunningId(), gameDataPacket);
        _players[notification.Network] = player;

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
                reason = _stringLocalizer[s => s.GameMessageFormat.ServerWhitelistedGameModes, new ArrayStringFormat<string>(_options.ServerOptions.WhitelistedGameModes)];
                break;
            }
        }

        if (playerCanJoin)
        {
            return _mediator.Publish(new PlayerJoin(player), cancellationToken);
        }

        player.Kick(reason);
        _logger.LogInformation("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.PlayerUnableToJoin, player.DisplayName, reason]);
        
        return ValueTask.CompletedTask;
    }

    private ValueTask HandleChatMessage(NewPacketReceived notification, CancellationToken cancellationToken)
    {
        foreach (var otherPlayer in GetPlayersEnumerable())
        {
            otherPlayer.SendPacket(notification.RawPacket);
        }
        
        return ValueTask.CompletedTask;
    }

    private ValueTask HandleGamestateMessage(NewPacketReceived notification, CancellationToken cancellationToken)
    {
        var gamestateMessagePacket = new GamestateMessagePacket(notification.RawPacket);
        var player = GetPlayerById(gamestateMessagePacket.Origin);

        foreach (var otherPlayer in GetPlayersEnumerable())
        {
            otherPlayer.SendPacket(new ChatMessagePacket(Origin.Server, _stringLocalizer[s => s.GameMessageFormat.GameStateMessage, player.DisplayName, gamestateMessagePacket.Message]));
        }
        
        _logger.LogInformation("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.GameStateMessage, player.DisplayName, gamestateMessagePacket.Message]);
        
        return ValueTask.CompletedTask;
    }
    
    private ValueTask HandleServerDataRequest(NewPacketReceived notification, CancellationToken cancellationToken)
    {
        notification.Network.SendPacket(new ServerInfoDataPacket(
            GetPlayerCount(),
            _options.ServerOptions.MaxPlayers,
            _options.ServerOptions.ServerName,
            _options.ServerOptions.ServerDescription,
            GetPlayerDisplayNames()));

        return ValueTask.CompletedTask;
    }

    #endregion

    #region Player Events

    public async ValueTask Handle(PlayerJoin notification, CancellationToken cancellationToken)
    {
        var player = notification.Player;

        player.SendPacket(new IdPacket(player.Id));

        await player.InitializePlayer(_globalWorld, cancellationToken).ConfigureAwait(false);

        foreach (var otherPlayer in GetPlayersEnumerable(null, true))
        {
            if (otherPlayer != player)
            {
                player.SendPacket(new CreatePlayerPacket(otherPlayer.Id));
                player.SendPacket(otherPlayer.ToGameDataPacket());
            }

            otherPlayer.SendPacket(new CreatePlayerPacket(player.Id));
            otherPlayer.SendPacket(player.ToGameDataPacket());
            otherPlayer.SendPacket(new ChatMessagePacket(-1, _stringLocalizer[s => s.GameMessageFormat.PlayerJoin, player.DisplayName]));
        }

        foreach (var welcomeMessage in _options.ServerOptions.WelcomeMessage)
        {
            player.SendPacket(new ChatMessagePacket(-1, welcomeMessage));
        }

        _logger.LogInformation("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.PlayerJoin, player.DisplayName]);
    }

    public ValueTask Handle(PlayerUpdated notification, CancellationToken cancellationToken)
    {
        var player = notification.Player;

        foreach (var otherPlayer in GetPlayersEnumerable(null, true))
        {
            otherPlayer.SendPacket(player.ToGameDataPacket());
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(PlayerLeft notification, CancellationToken cancellationToken)
    {
        var player = notification.Player;
        var reason = notification.Reason;

        foreach (var otherPlayer in GetPlayersEnumerable(null, true))
        {
            otherPlayer.SendPacket(new DestroyPlayerPacket(player.Id));
            otherPlayer.SendPacket(new ChatMessagePacket(-1, _stringLocalizer[s => s.GameMessageFormat.PlayerLeft, player.DisplayName]));
        }
        
        _logger.LogInformation("{Message}", reason is null ? _stringLocalizer[s => s.ConsoleMessageFormat.PlayerLeft, player.DisplayName] : _stringLocalizer[s => s.ConsoleMessageFormat.PlayerLeftWithReason, player.DisplayName, notification.Reason]);
        
        if (notification.Player is IDisposable disposable)
        {
            disposable.Dispose();
        }
        
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Player Functions

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

    #region World Events

    public ValueTask<Unit> Handle(StartGlobalWorld command, CancellationToken cancellationToken)
    {
        _globalWorld.StartWorld();
        return Unit.ValueTask;
    }

    public ValueTask<Unit> Handle(StopGlobalWorld command, CancellationToken cancellationToken)
    {
        _globalWorld.StopWorld();
        return Unit.ValueTask;
    }

    #endregion
}