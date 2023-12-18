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
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

internal sealed partial class PokemonServerClient : IPokemonServerClient, IDisposable
{
    public IPAddress RemoteIpAddress { get; }

    private readonly ILogger _logger;
    private readonly IPokemonServerOptions _options;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly IMediator _mediator;
    private readonly TcpClient _tcpClient;
    
    private readonly CancellationTokenSource _sendCompleteTokenSource;
    private CancellationToken SendCompleteToken => _sendCompleteTokenSource.Token;

    private readonly CancellationTokenSource _disconnectedTokenSource;
    private CancellationToken DisconnectedToken => _disconnectedTokenSource.Token;

    private readonly Timer _pingCheckTimer;

    private readonly BlockingCollection<IRawPacket> _handleGameDataPacketQueue = new();
    private readonly BlockingCollection<Task> _handlePacketHandlerExceptionQueue = new();
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
        _tcpClient = tcpClient;

        _sendCompleteTokenSource = new CancellationTokenSource();
        _disconnectedTokenSource = new CancellationTokenSource();

        RemoteIpAddress = (_tcpClient.Client.RemoteEndPoint as IPEndPoint)!.Address;

        _pingCheckTimer = new Timer(PingCheckTimerCallback, null, TimeSpan.FromSeconds(_options.ServerOptions.NoPingKickTime), Timeout.InfiniteTimeSpan);

        Task.Factory.StartNew(PacketDataProducer, TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(HandleGameDataPacketQueue, TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(HandlePacketHandlerExceptionQueue, TaskCreationOptions.LongRunning);
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
        if (waitForCompletion)
        {
            _sendingPacketQueue.CompleteAdding();
            SendCompleteToken.WaitHandle.WaitOne();
        }
        
        _disconnectedTokenSource.Cancel();
        _pingCheckTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        _handleGameDataPacketQueue.CompleteAdding();
        _handlePacketHandlerExceptionQueue.CompleteAdding();
        _sendingPacketQueue.CompleteAdding();

        _tcpClient.Close();

        PrintDebug(RemoteIpAddress, _stringLocalizer[s => s.ConsoleMessageFormat.ClientDisconnected]);

        Task.Run(() => _mediator.Publish(new ClientDisconnected(this, reason), CancellationToken.None).AsTask(), CancellationToken.None).GetAwaiter().GetResult();
    }

    private void PingCheckTimerCallback(object? state)
    {
        Disconnect(_stringLocalizer[s => s.ConsoleMessageFormat.ClientReceivedNoPing]);
    }

    private void PacketDataProducer()
    {
        try
        {
            using var streamReader = new StreamReader(_tcpClient.GetStream(), Encoding.UTF8, false, _tcpClient.ReceiveBufferSize, true);

            while (_tcpClient.Connected)
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
                        _handlePacketHandlerExceptionQueue.Add(Task.Run(() => _mediator.Publish(new NewPacketReceived(this, rawPacket), DisconnectedToken).AsTask(), DisconnectedToken), DisconnectedToken);
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
        try
        {
            foreach (var rawPacket in _handleGameDataPacketQueue.GetConsumingEnumerable())
            {
                _mediator.Publish(new NewPacketReceived(this, rawPacket), DisconnectedToken).AsTask().Wait(DisconnectedToken);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            var reason = _stringLocalizer[s => s.ConsoleMessageFormat.ServerUncaughtExceptionThrown];
            PrintDebugWithException(exception, RemoteIpAddress, reason);
            Disconnect(reason);
        }
    }

    private void HandlePacketHandlerExceptionQueue()
    {
        foreach (var task in _handlePacketHandlerExceptionQueue.GetConsumingEnumerable())
        {
            if (task.IsCanceled || task.IsCompletedSuccessfully) continue;

            if (task.IsFaulted)
            {
                var reason = _stringLocalizer[s => s.ConsoleMessageFormat.ServerUncaughtExceptionThrown];
                PrintDebugWithException(task.Exception, RemoteIpAddress, reason);
                Disconnect(reason);
                return;
            }

            _handlePacketHandlerExceptionQueue.Add(task, DisconnectedToken);
        }
    }

    private void SendingQueueConsumer()
    {
        try
        {
            using var streamWriter = new StreamWriter(_tcpClient.GetStream(), Encoding.UTF8, _tcpClient.SendBufferSize, true);
            streamWriter.AutoFlush = true;

            foreach (var rawPacket in _sendingPacketQueue.GetConsumingEnumerable(DisconnectedToken))
            {
                if (!_tcpClient.Connected) return;

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
        _tcpClient.Dispose();
        _disconnectedTokenSource.Dispose();
        _pingCheckTimer.Dispose();
        _handleGameDataPacketQueue.Dispose();
        _handlePacketHandlerExceptionQueue.Dispose();
        _sendingPacketQueue.Dispose();
    }
}