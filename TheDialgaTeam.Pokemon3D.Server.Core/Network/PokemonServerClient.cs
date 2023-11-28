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

using System.Net;
using System.Net.Sockets;
using System.Text;
using Mediator;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

internal sealed partial class PokemonServerClient : IPokemonServerClient
{
    public IPAddress RemoteIpAddress { get; }

    private readonly ILogger<PokemonServerClient> _logger;
    private readonly IPokemonServerOptions _options;
    private readonly IMediator _mediator;
    private readonly TcpClient _tcpClient;

    private readonly StreamWriter _streamWriter;
    private readonly object _streamWriterLock = new();
    
    private readonly IDisposable[] _disposables;

    private DateTime _lastValidPackage = DateTime.Now;

    public PokemonServerClient(
        ILogger<PokemonServerClient> logger, 
        IPokemonServerOptions options, 
        IMediator mediator, 
        TcpClient tcpClient)
    {
        _logger = logger;
        _options = options;
        _mediator = mediator;
        _tcpClient = tcpClient;
        _streamWriter = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8, tcpClient.SendBufferSize);

        RemoteIpAddress = (_tcpClient.Client.RemoteEndPoint as IPEndPoint)!.Address;

        _disposables = new IDisposable[]
        {
            Task.Factory.StartNew(RunReadingTask, TaskCreationOptions.LongRunning).Unwrap(),
            Task.Factory.StartNew(RunConnectionCheckTask, TaskCreationOptions.LongRunning).Unwrap()
        };
    }

    public void SendPackage(RawPacket rawPacket)
    {
        lock (_streamWriterLock)
        {
            try
            {
                var packageData = rawPacket.ToRawPacketString();

                PrintSendRawPacket(RemoteIpAddress, packageData);

                _streamWriter.WriteLine(packageData);
                _streamWriter.Flush();
            }
            catch
            {
                PrintWriteSocketIssue(RemoteIpAddress);
                throw;
            }
        }
    }

    public ValueTask DisconnectAsync()
    {
        _tcpClient.Close();
        PrintDisconnected(RemoteIpAddress);
        return _mediator.Publish(new Disconnected(this));
    }

    private async Task RunReadingTask()
    {
        using var streamReader = new StreamReader(_tcpClient.GetStream(), Encoding.UTF8, false, _tcpClient.ReceiveBufferSize, true);

        while (_tcpClient.Connected)
        {
            try
            {
                var rawData = await streamReader.ReadLineAsync().ConfigureAwait(false);
                
                if (rawData == null)
                {
                    await DisconnectAsync().ConfigureAwait(false);
                    return;
                }

                PrintReceiveRawPacket(RemoteIpAddress, rawData);

                if (!RawPacket.TryParse(rawData, out var package))
                {
                    PrintInvalidPacketReceive(RemoteIpAddress);
                }
                else
                {
                    _lastValidPackage = DateTime.Now;

                    try
                    {
                        await _mediator.Publish(new NewPacketReceived(this, package));
                    }
                    catch (Exception ex)
                    {
                        PrintPacketHandlerError(ex, RemoteIpAddress, ex.Message);
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                PrintOutOfMemory(RemoteIpAddress);
            }
            catch (IOException)
            {
                PrintReadSocketIssue(RemoteIpAddress);
                await DisconnectAsync();
            }
        }
    }

    private async Task RunConnectionCheckTask()
    {
        while (_tcpClient.Connected)
        {
            if ((DateTime.Now - _lastValidPackage).TotalSeconds > _options.ServerOptions.NoPingKickTime.TotalSeconds)
            {
                // Most likely disconnected, so let's destroy it.
                await DisconnectAsync();
            }

            await Task.Delay(1000);
        }
    }

    [LoggerMessage(LogLevel.Trace, "[{ipAddress}] Receive raw packet data: {rawData}")]
    private partial void PrintReceiveRawPacket(IPAddress ipAddress, string rawData);

    [LoggerMessage(LogLevel.Trace, "[{ipAddress}] Send raw packet data: {rawData}")]
    private partial void PrintSendRawPacket(IPAddress ipAddress, string rawData);

    [LoggerMessage(LogLevel.Debug, "[{ipAddress}] Unable to allocate buffer for the packet data due to insufficient memory")]
    private partial void PrintOutOfMemory(IPAddress ipAddress);

    [LoggerMessage(LogLevel.Debug, "[{ipAddress}] Unable to read data from this network")]
    private partial void PrintReadSocketIssue(IPAddress ipAddress);

    [LoggerMessage(LogLevel.Debug, "[{ipAddress}] Unable to write data from this network")]
    private partial void PrintWriteSocketIssue(IPAddress ipAddress);

    [LoggerMessage(LogLevel.Debug, "[{ipAddress}] Invalid packet received")]
    private partial void PrintInvalidPacketReceive(IPAddress ipAddress);

    [LoggerMessage(LogLevel.Debug, "[{ipAddress}] Disconnected")]
    private partial void PrintDisconnected(IPAddress ipAddress);
    
    [LoggerMessage(LogLevel.Debug, "[{ipAddress}] {message}")]
    private partial void PrintPacketHandlerError(Exception exception, IPAddress ipAddress, string message);

    public void Dispose()
    {
        _tcpClient.Dispose();

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}