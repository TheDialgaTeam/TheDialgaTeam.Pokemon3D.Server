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
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Queries;
using TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public sealed partial class NetworkContainer :
    INotificationHandler<Connected>,
    INotificationHandler<Disconnected>,
    INotificationHandler<NewPacketReceived>
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly IPokemonServerOptions _options;
    
    private readonly List<IPokemonServerClient> _tcpClientNetworks = new();
    private readonly object _clientLock = new();
    
    public NetworkContainer(ILogger<NetworkContainer> logger, IMediator mediator, IPokemonServerOptions options)
    {
        _logger = logger;
        _mediator = mediator;
        _options = options;
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
                var playerCount = await _mediator.Send(new GetPlayerCount(), cancellationToken).ConfigureAwait(false);
                var packet = new ServerInfoDataPacket(playerCount, _options.ServerOptions.MaxPlayers, _options.ServerOptions.ServerName, _options.ServerOptions.ServerDescription, Array.Empty<string>());
                notification.Network.SendPacket(packet.ToRawPacket());
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
                    
            // Check Server Space Limit.
            var playerCount = await _mediator.Send(new GetPlayerCount(), cancellationToken).ConfigureAwait(false);

            if (playerCount >= _options.ServerOptions.MaxPlayers)
            {
                var reason = _options.GetLocalizedString("SERVER_IS_FULL");
                await notification.Network.KickAsync(reason).ConfigureAwait(false);
                PrintServerMessage(_options.GetLocalizedString("SERVER_UNABLE_TO_JOIN", gameDataPacket, reason));
                return;
            }
        }
        else
        {
            // This is updating existing player.
        }
    }

    [LoggerMessage(LogLevel.Information, "[Server] {message}")]
    private partial void PrintServerMessage(string message);
}