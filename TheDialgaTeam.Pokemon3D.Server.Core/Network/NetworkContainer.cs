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

using Mediator;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Queries;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public sealed class NetworkContainer :
    INotificationHandler<Connected>,
    INotificationHandler<Disconnected>,
    INotificationHandler<NewPacketReceived>
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly IPokemonServerOptions _options;
    private readonly IStringLocalizer _stringLocalizer;

    private readonly List<IPokemonServerClient> _tcpClientNetworks = new();
    private readonly object _clientLock = new();
    
    public NetworkContainer(
        ILogger<NetworkContainer> logger, 
        IMediator mediator, 
        IPokemonServerOptions options,
        IStringLocalizer stringLocalizer)
    {
        _logger = logger;
        _mediator = mediator;
        _options = options;
        _stringLocalizer = stringLocalizer;
    }

    private void AddClient(IPokemonServerClient network)
    {
        lock (_clientLock)
        {
            _tcpClientNetworks.Add(network);
        }
    }

    private void RemoveClient(IPokemonServerClient network)
    {
        lock (_clientLock)
        {
            _tcpClientNetworks.Remove(network);
        }
    }

    public ValueTask Handle(Connected notification, CancellationToken cancellationToken)
    {
        AddClient(notification.PokemonServerClient);
        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(Disconnected notification, CancellationToken cancellationToken)
    {
        notification.PokemonServerClient.Dispose();
        RemoveClient(notification.PokemonServerClient);
        return ValueTask.CompletedTask;
    }

    public async ValueTask Handle(NewPacketReceived notification, CancellationToken cancellationToken)
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
                var serverInfoDataPacket = await _mediator.Send(new GetServerInfoData(), cancellationToken).ConfigureAwait(false);
                notification.Network.SendPacket(serverInfoDataPacket.ToRawPacket());
                break;
            }
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
            var playerCount = await _mediator.Send(new GetPlayerCount(), cancellationToken).ConfigureAwait(false);
            
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
        }
        else
        {
            // This is updating existing player.
        }
    }
}