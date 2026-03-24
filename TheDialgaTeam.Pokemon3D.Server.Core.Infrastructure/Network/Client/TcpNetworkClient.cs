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
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Packets;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Client;

public sealed class TcpNetworkClient(TcpClient tcpClient) : INetworkClient
{
    public IPEndPoint RemoteEndPoint => tcpClient.Client.RemoteEndPoint as IPEndPoint ?? throw new InvalidOperationException();

    public IObservable<IRawPacket> ObservePackets => _rawPacketSubject.AsObservable();
    public IObservable<Unit> ObserveDisconnected => _disconnectedSubject.AsObservable();

    private readonly StreamReader _reader = new(tcpClient.GetStream(), Encoding.UTF8, false, tcpClient.ReceiveBufferSize, true);
    private readonly StreamWriter _writer = new(tcpClient.GetStream(), Encoding.UTF8, tcpClient.SendBufferSize, true);
    
    private readonly SemaphoreSlim _writeSemaphoreSlim = new(1, 1);

    private readonly Subject<IRawPacket> _rawPacketSubject = new();
    private readonly Subject<Unit> _disconnectedSubject = new();
    
    private IDisposable? _listeningTask;

    public void StartListening()
    {
        _listeningTask ??= Observable.FromAsync(async token =>
        {
            while (tcpClient.Connected && !token.IsCancellationRequested)
            {
                try
                {
                    var rawPacketString = await _reader.ReadLineAsync(token).ConfigureAwait(false);
                    
                    if (rawPacketString == null)
                    {
                        _rawPacketSubject.OnCompleted();
                        _disconnectedSubject.OnNext(Unit.Default);
                        _disconnectedSubject.OnCompleted();
                        break;
                    }

                    if (!RawPacket.TryParse(rawPacketString, out var rawPacket)) continue;
                    
                    _rawPacketSubject.OnNext(rawPacket);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _rawPacketSubject.OnError(exception);
                }
            }
        }).Subscribe();
    }
    
    public async Task SendPacketAsync(IRawPacket packet, CancellationToken cancellationToken = default)
    {
        try
        {
            await _writeSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
            await _writer.WriteLineAsync(packet.ToRawPacketString()).ConfigureAwait(false);
            await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeSemaphoreSlim.Release();
        }
    }

    public void Dispose()
    {
        _listeningTask?.Dispose();
        _writeSemaphoreSlim.Dispose();
        tcpClient.Dispose();
    }
}