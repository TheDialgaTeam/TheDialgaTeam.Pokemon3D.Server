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
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Network.Client;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Packets;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Client;

internal class TcpNetworkClient : INetworkClient
{
    public IPEndPoint IPEndPoint => _tcpClient.Client.RemoteEndPoint as IPEndPoint ?? throw new InvalidOperationException();

    private readonly TcpClient _tcpClient;
    private readonly RawPacketStream _rawPacketStream;

    public TcpNetworkClient(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        _rawPacketStream = new RawPacketStream(_tcpClient.GetStream(), _tcpClient.ReceiveBufferSize, _tcpClient.SendBufferSize);
    }

    public IObservable<IRawPacket> ObservePackets()
    {
        return Observable.Create<IRawPacket>(async (observer, cancellationToken) =>
        {
            while (_tcpClient.Connected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var packet = await _rawPacketStream.ReadPacketAsync(cancellationToken).ConfigureAwait(false);

                    if (packet != null)
                    {
                        observer.OnNext(packet);
                    }
                    else
                    {
                        observer.OnCompleted();
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    observer.OnError(ex);
                }
            }

            return Disposable.Empty;
        });
    }

    public void SendPacket(IRawPacket packet)
    {
        _rawPacketStream.WritePacket(packet);
    }

    public void Dispose()
    {
        _tcpClient.Dispose();
    }
}