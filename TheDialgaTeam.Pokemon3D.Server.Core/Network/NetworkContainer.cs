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
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Queries;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public sealed class NetworkContainer :
    INotificationHandler<ClientConnected>,
    INotificationHandler<ClientDisconnected>,
    INotificationHandler<NewPacketReceived>
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
        notification.PokemonServerClient.Dispose();

        if (_players.TryRemove(notification.PokemonServerClient, out var player))
        {
            if (player is not null)
            {
                _runningIds.Add(player.Id);
                _logger.LogInformation("{Message}", notification.Reason is null ? _stringLocalizer[s => s.ConsoleMessageFormat.PlayerLeft, player] : _stringLocalizer[s => s.ConsoleMessageFormat.PlayerLeftWithReason, player, notification.Reason]);
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

                case PacketType.ServerDataRequest:
                {
                    notification.Network.SendPacket(new ServerInfoDataPacket(
                        GetPlayerCount(),
                        _options.ServerOptions.MaxPlayers,
                        _options.ServerOptions.ServerName,
                        _options.ServerOptions.ServerDescription,
                        GetPlayerDisplayNames()).ToRawPacket());
                    break;
                }
            }
        }
        catch
        {
            await notification.Network.KickAsync(_stringLocalizer[s => s.GameMessageFormat.ServerError]).ConfigureAwait(false);
        }
    }

    #region Player Related Functions

    private int GetPlayerCount()
    {
        return _players.Values.Count(player => player is not null);
    }

    private IPlayer GetPlayerById(int id)
    {
        return _players.Values.Single(player => player is not null && player.Id == id)!;
    }
    
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

    private string[] GetPlayerDisplayNames()
    {
        var playerCount = GetPlayerCount();

        if (playerCount > 20)
        {
            return _players.Values
                .Where(player => player is not null)
                .Take(20)
                .Select(player => player!.DisplayName)
                .Append($"(And {playerCount - 20} more)")
                .ToArray();
        }

        return _players.Values
            .Where(player => player is not null)
            .Select(player => player!.DisplayName)
            .ToArray();
    }

    private IEnumerable<IPlayer> GetPlayers(int excludeId = 0)
    {
        return _players.Values.Where(player => player is not null && player.Id != excludeId)!;
    }

    #endregion
    
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
                    reason = _stringLocalizer[s => s.GameMessageFormat.ServerWhitelistedGameModes, string.Join(", ", _options.ServerOptions.WhitelistedGameModes)];
                    break;
                }
            }

            if (!playerCanJoin)
            {
                await notification.Network.KickAsync(reason).ConfigureAwait(false);
                _logger.LogInformation("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.PlayerUnableToJoin, gameDataPacket, reason]);
                return;
            }

            // If all okay, let the player join in by generating an id and providing the world.
            _logger.LogInformation("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.PlayerJoin, gameDataPacket]);
            
            var player = _playerFactory.CreatePlayer(GetNextRunningId(), gameDataPacket);
            _players[notification.Network] = player;
            
            notification.Network.SendPacket(new IdPacket(player.Id).ToRawPacket());

            var world = await _mediator.Send(new GetGlobalWorld(), cancellationToken).ConfigureAwait(false);
            notification.Network.SendPacket(world.GetWorldDataPacket().ToRawPacket());

            foreach (var otherPlayer in GetPlayers(player.Id))
            {
                notification.Network.SendPacket(otherPlayer.ToGameDataPacket().ToRawPacket());
            }

            foreach (var welcomeMessage in _options.ServerOptions.WelcomeMessage)
            {
                notification.Network.SendPacket(new ChatMessagePacket(-1, welcomeMessage).ToRawPacket());
            }
        }
        else
        {
            // This is updating existing player.
            await GetPlayerById(notification.RawPacket.Origin).ApplyGameDataAsync(notification.RawPacket).ConfigureAwait(false);
        }
    }
}