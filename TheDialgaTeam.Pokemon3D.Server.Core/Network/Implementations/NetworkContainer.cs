﻿// Pokemon 3D Server Client
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
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Implementations.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Implementations.Packets.Types;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Queries;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Implementations;

internal sealed class NetworkContainer : 
    INotificationHandler<Connected>, 
    INotificationHandler<Disconnected>,
    INotificationHandler<NewPacketReceived>
{
    private readonly IMediator _mediator;
    private readonly IPokemonServerOptions _options;
    
    private readonly List<IPokemonServerClient> _tcpClientNetworks = new();
    private readonly object _clientLock = new();

    public NetworkContainer(IMediator mediator, IPokemonServerOptions options)
    {
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
        switch (notification.Packet.PacketType)
        {
            case PacketType.ServerDataRequest:
            {
                var playerCount = await _mediator.Send(new GetPlayerCount(), cancellationToken).ConfigureAwait(false);
                var packet = new ServerInfoDataPacket(playerCount, _options.ServerOptions.MaxPlayers, _options.ServerOptions.ServerName, _options.ServerOptions.ServerDescription, Array.Empty<string>());
                notification.Network.SendPackage(packet);
                break;
            }
        }
    }
}