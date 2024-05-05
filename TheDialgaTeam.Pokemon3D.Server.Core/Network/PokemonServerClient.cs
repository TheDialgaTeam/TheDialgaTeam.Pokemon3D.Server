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
using System.Net;
using System.Net.Sockets;
using System.Text;
using Mediator;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Domain.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using RawPacket = TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets.RawPacket;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

internal sealed partial class PokemonServerClient : IPokemonServerClient, IDisposable
{
    public IPAddress RemoteIpAddress { get; }

    private readonly ILogger _logger;
    private readonly IPokemonServerOptions _options;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly IMediator _mediator;
    private readonly NetworkStream _networkStream;
    
    private readonly CancellationTokenSource _sendCompleteTokenSource;
    private CancellationToken SendCompleteToken => _sendCompleteTokenSource.Token;

    private readonly CancellationTokenSource _disconnectedTokenSource;
    private CancellationToken DisconnectedToken => _disconnectedTokenSource.Token;

    private readonly Timer _pingCheckTimer;

    private readonly BlockingCollection<IRawPacket> _handleGameDataPacketQueue = new();
    private readonly BlockingCollection<IRawPacket> _sendingPacketQueue = new();

    public PokemonServerClient(
        ILogger<PokemonServerClient> logger,
        IPokemonServerOptions options,
        IStringLocalizer stringLocalizer,
        IMediator mediator,
        TcpClient tcpClient)
    {
        _logger = logger;
        _options = options;
        _stringLocalizer = stringLocalizer;
        _mediator = mediator;
        _networkStream = tcpClient.GetStream();

        _sendCompleteTokenSource = new CancellationTokenSource();
        _disconnectedTokenSource = new CancellationTokenSource();
        
        RemoteIpAddress = (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address ?? IPAddress.None;

        _pingCheckTimer = new Timer(PingCheckTimerCallback, null, TimeSpan.FromSeconds(_options.ServerOptions.NoPingKickTime), Timeout.InfiniteTimeSpan);

        Task.Factory.StartNew(PacketDataReader, TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(HandleGameDataPacketQueue, TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(SendingQueueConsumer, TaskCreationOptions.LongRunning);
    }

    public void SendPacket(IPacket packet)
    {
        SendPacket(packet.ToServerRawPacket());
    }

    public void SendPacket(IRawPacket rawPacket)
    {
        if (DisconnectedToken.IsCancellationRequested) return;
        _sendingPacketQueue.Add(rawPacket, DisconnectedToken);
    }

    public void Disconnect(string? reason = null, bool waitForCompletion = false)
    {
        if (DisconnectedToken.IsCancellationRequested) return;
        
        _pingCheckTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _sendingPacketQueue.CompleteAdding();
        _handleGameDataPacketQueue.CompleteAdding();
        
        if (waitForCompletion)
        {
            SendCompleteToken.WaitHandle.WaitOne();
        }
        
        _disconnectedTokenSource.Cancel();
        _networkStream.Close();

        PrintDebug(RemoteIpAddress, _stringLocalizer[s => s.ConsoleMessageFormat.ClientDisconnected]);

        Task.Run(() => _mediator.Publish(new ClientDisconnected(this, reason), CancellationToken.None).AsTask(), CancellationToken.None);
    }

    private void PingCheckTimerCallback(object? state)
    {
        Disconnect(_stringLocalizer[s => s.ConsoleMessageFormat.ClientReceivedNoPing]);
    }

    private void PacketDataReader()
    {
        try
        {
            using var streamReader = new StreamReader(_networkStream, Encoding.UTF8, false, _networkStream.Socket.ReceiveBufferSize, true);

            while (_networkStream.Socket.Connected)
            {
                var rawData = streamReader.ReadLine();

                if (rawData == null)
                {
                    Disconnect(_stringLocalizer[s => s.ConsoleMessageFormat.ClientDisconnected]);
                    return;
                }

                PrintTrace(RemoteIpAddress, _stringLocalizer[s => s.ConsoleMessageFormat.ClientReceivedRawPacket, rawData]);

                if (!RawPacket.TryParse(rawData, out var rawPacket))
                {
                    PrintDebug(RemoteIpAddress, _stringLocalizer[s => s.ConsoleMessageFormat.ClientReceivedInvalidPacket]);
                }
                else
                {
                    _pingCheckTimer.Change(TimeSpan.FromSeconds(_options.ServerOptions.NoPingKickTime), Timeout.InfiniteTimeSpan);

                    if (rawPacket.PacketType == PacketType.GameData)
                    {
                        _handleGameDataPacketQueue.Add(rawPacket, DisconnectedToken);
                    }
                    else
                    {
                        Task.Run(() => _mediator.Publish(new NewPacketReceived(this, rawPacket), DisconnectedToken).AsTask(), DisconnectedToken);
                    }
                }
            }
        }
        catch (IOException)
        {
            var reason = _stringLocalizer[s => s.ConsoleMessageFormat.ClientReadSocketIssue];
            PrintDebug(RemoteIpAddress, reason);
            Disconnect(reason);
        }
    }

    private void HandleGameDataPacketQueue()
    {
        foreach (var rawPacket in _handleGameDataPacketQueue.GetConsumingEnumerable())
        {
            _mediator.Publish(new NewPacketReceived(this, rawPacket), DisconnectedToken).AsTask().Wait(DisconnectedToken);
        }
    }

    private void SendingQueueConsumer()
    {
        try
        {
            using var streamWriter = new StreamWriter(_networkStream, Encoding.UTF8, _networkStream.Socket.SendBufferSize, true);
            streamWriter.AutoFlush = true;

            foreach (var rawPacket in _sendingPacketQueue.GetConsumingEnumerable())
            {
                if (!_networkStream.Socket.Connected) return;

                var packageData = rawPacket.ToRawPacketString();
                streamWriter.WriteLine(packageData);

                PrintTrace(RemoteIpAddress, _stringLocalizer[s => s.ConsoleMessageFormat.ClientSentRawPacket, packageData]);
            }
        }
        catch (IOException)
        {
            var reason = _stringLocalizer[s => s.ConsoleMessageFormat.ClientWriteSocketIssue];
            PrintDebug(RemoteIpAddress, reason);
            Disconnect(reason);
        }
        finally
        {
            _sendCompleteTokenSource.Cancel();
        }
    }

    [LoggerMessage(LogLevel.Debug, "[Client:{RemoteIpAddress}] {Message}")]
    private partial void PrintDebug(IPAddress remoteIpAddress, string message);

    [LoggerMessage(LogLevel.Debug, "[Client:{RemoteIpAddress}] {Message}")]
    private partial void PrintDebugWithException(Exception exception, IPAddress remoteIpAddress, string message);

    [LoggerMessage(LogLevel.Trace, "[Client:{RemoteIpAddress}] {Message}")]
    private partial void PrintTrace(IPAddress remoteIpAddress, string message);

    public void Dispose()
    {
        _networkStream.Dispose();
        _sendCompleteTokenSource.Dispose();
        _disconnectedTokenSource.Dispose();
        _pingCheckTimer.Dispose();
        _handleGameDataPacketQueue.Dispose();
        _sendingPacketQueue.Dispose();
    }
}