// Pokemon 3D Server Client
// Copyright (C) 2026 Yong Jian Ming
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

using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Client;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network;

public sealed partial class NetworkClientHandler : IDisposable
{
    private readonly INetworkClient _client;
    private readonly CompositeDisposable _disposables = new();

    public NetworkClientHandler(
        ILogger<NetworkClientHandler> logger,
        NetworkClientPacketHandler packetHandler,
        NetworkClientContainer clientContainer,
        INetworkClient client)
    {
        _client = client;
        
        client.ObservePackets
            .Do(packet => LogIncomingPacket(logger, client.RemoteEndPoint.Address, packet.ToRawPacketString()))
            .Subscribe(packet => packetHandler.HandlePacket(client, packet))
            .DisposeWith(_disposables);

        client.ObserveDisconnected
            .Do(_ => LogDisconnected(logger, client.RemoteEndPoint.Address))
            .Subscribe(_ => clientContainer.RemoveConnection(client))
            .DisposeWith(_disposables);
        
        client.StartListening();
    }

    [LoggerMessage(LogLevel.Trace, "[{remoteIpAddress}] Incoming Packet: {packet}")]
    private static partial void LogIncomingPacket(ILogger<NetworkClientHandler> logger, IPAddress remoteIpAddress, string packet);

    [LoggerMessage(LogLevel.Trace, "[{remoteIpAddress}] Disconnected")]
    private static partial void LogDisconnected(ILogger<NetworkClientHandler> logger, IPAddress remoteIpAddress);

    public void Dispose()
    {
        _client.Dispose();
        _disposables.Dispose();
    }
}