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
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packages;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Clients;

internal sealed partial class TcpClientNetwork : IClientNetwork
{
    public IPAddress RemoteIpAddress { get; }

    private readonly ILogger<TcpClientNetwork> _logger;
    private readonly IMediator _mediator;
    private readonly TcpClient _tcpClient;

    private readonly Task _readingTask;
    private readonly Task _writingTask;
    private readonly Task _connectionCheckTask;

    private readonly ConcurrentQueue<Package> _packages = new();

    private DateTime _lastValidPackage = DateTime.Now;

    public TcpClientNetwork(ILogger<TcpClientNetwork> logger, IMediator mediator, TcpClient tcpClient)
    {
        _logger = logger;
        _mediator = mediator;
        _tcpClient = tcpClient;

        RemoteIpAddress = (_tcpClient.Client.RemoteEndPoint as IPEndPoint)!.Address;

        _readingTask = Task.Factory.StartNew(RunReadingTask, TaskCreationOptions.LongRunning).Unwrap();
        _writingTask = Task.Factory.StartNew(RunWritingTask, TaskCreationOptions.LongRunning).Unwrap();
        _connectionCheckTask = Task.Factory.StartNew(RunConnectionCheckTask, TaskCreationOptions.LongRunning).Unwrap();
    }

    public void EnqueuePackage(Package package)
    {
        _packages.Enqueue(package);
    }

    public Task DisconnectAsync()
    {
        _tcpClient.Close();
        PrintDisconnected(RemoteIpAddress);
        return _mediator.PublishAsync(new DisconnectedEventArgs(this));
    }

    private async Task RunReadingTask()
    {
        var streamReader = new StreamReader(_tcpClient.GetStream());

        while (_tcpClient.Connected)
        {
            try
            {
                var rawData = await streamReader.ReadLineAsync().ConfigureAwait(false);
                if (rawData == null) break;

                PrintReceiveRawPackage(RemoteIpAddress, rawData);

                var package = new Package(rawData);

                if (!package.IsValid)
                {
                    PrintInvalidPackageReceive(RemoteIpAddress);
                }
                else
                {
                    _lastValidPackage = DateTime.Now;
                    _ = Task.Run(() => _mediator.PublishAsync(new NewPackageReceivedEventArgs(this, package)));
                }
            }
            catch (OutOfMemoryException)
            {
                PrintOutOfMemory(RemoteIpAddress);
            }
            catch (IOException)
            {
                PrintReadSocketIssue(RemoteIpAddress);
            }
        }
    }

    private async Task RunWritingTask()
    {
        var streamWriter = new StreamWriter(_tcpClient.GetStream());

        while (_tcpClient.Connected)
        {
            while (_packages.TryDequeue(out var package))
            {
                try
                {
                    var packageData = package.ToString();

                    PrintSendRawPackage(RemoteIpAddress, packageData);

                    await streamWriter.WriteLineAsync(packageData).ConfigureAwait(false);
                    await streamWriter.FlushAsync().ConfigureAwait(false);

                    package.TaskCompletionSource.SetResult();
                }
                catch (IOException exception)
                {
                    PrintWriteSocketIssue(RemoteIpAddress);
                    package.TaskCompletionSource.SetException(exception);
                }
            }

            await Task.Delay(1).ConfigureAwait(false);
        }
    }

    private async Task RunConnectionCheckTask()
    {
        while (_tcpClient.Connected)
        {
            if ((DateTime.Now - _lastValidPackage).TotalSeconds > 10)
            {
                // Most likely disconnected, so let's destroy it.
                await DisconnectAsync().ConfigureAwait(false);
            }

            await Task.Delay(1000).ConfigureAwait(false);
        }
    }

    [LoggerMessage(Level = LogLevel.Trace, Message = "[{ipAddress}] Receive raw package data: {rawData}")]
    private partial void PrintReceiveRawPackage(IPAddress ipAddress, string rawData);

    [LoggerMessage(Level = LogLevel.Trace, Message = "[{ipAddress}] Send raw package data: {rawData}")]
    private partial void PrintSendRawPackage(IPAddress ipAddress, string rawData);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[{ipAddress}] Unable to allocate buffer for the package data due to insufficient memory")]
    private partial void PrintOutOfMemory(IPAddress ipAddress);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[{ipAddress}] Unable to read data from this network")]
    private partial void PrintReadSocketIssue(IPAddress ipAddress);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[{ipAddress}] Unable to write data from this network")]
    private partial void PrintWriteSocketIssue(IPAddress ipAddress);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[{ipAddress}] Invalid package received")]
    private partial void PrintInvalidPackageReceive(IPAddress ipAddress);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[{ipAddress}] Disconnected")]
    private partial void PrintDisconnected(IPAddress ipAddress);

    public void Dispose()
    {
        _tcpClient.Dispose();
        _readingTask.Dispose();
        _writingTask.Dispose();
        _connectionCheckTask.Dispose();
    }
}