using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Client.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packages;
using TheDialgaTeam.Pokemon3D.Server.Core.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Clients;

public sealed partial class TcpClientNetwork
{
    public event EventHandler<NewPackageReceivedEventArgs>? NewPackageReceived;

    public event EventHandler<DisconnectedEventArgs>? Disconnected;

    public bool IsConnected => _tcpClient.Connected;

    public IPAddress RemoteIpAddress { get; }

    public Player.Player? Player { get; internal set; }
    
    private readonly ILogger _logger;
    private readonly IPokemonServerOptions _options;

    private readonly TcpClient _tcpClient;

    private Task? _readingTask;
    private Task? _writingTask;
    private Task? _connectionCheckTask;

    private readonly ConcurrentQueue<Package> _packages = new();

    private DateTime _lastValidPackage = DateTime.Now;

    public TcpClientNetwork(ILogger logger, IPokemonServerOptions options, TcpClient tcpClient)
    {
        _logger = logger;
        _options = options;
        _tcpClient = tcpClient;

        RemoteIpAddress = (_tcpClient.Client.RemoteEndPoint as IPEndPoint)!.Address;
    }

    public void Start()
    {
        _readingTask = Task.Factory.StartNew(RunReadingTask, TaskCreationOptions.LongRunning).Unwrap();
        _writingTask = Task.Factory.StartNew(RunWritingTask, TaskCreationOptions.LongRunning).Unwrap();
        _connectionCheckTask = Task.Factory.StartNew(RunConnectionCheckTask, TaskCreationOptions.LongRunning).Unwrap();
    }

    public void Disconnect()
    {
        _tcpClient.Close();
        PrintDisconnected(RemoteIpAddress);
        Disconnected?.Invoke(this, new DisconnectedEventArgs { Network = this });
    }

    public void EnqueuePackage(Package package)
    {
        _packages.Enqueue(package);
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
                    _ = Task.Run(() => NewPackageReceived?.Invoke(this, new NewPackageReceivedEventArgs { Network = this, Package = package }));
                }
            }
            catch (OutOfMemoryException)
            {
                PrintOutOfMemory(RemoteIpAddress);
                Disconnect();
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
                Disconnect();
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
}