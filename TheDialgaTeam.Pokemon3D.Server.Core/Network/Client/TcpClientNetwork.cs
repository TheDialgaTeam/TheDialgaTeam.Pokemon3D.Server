using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packages;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Client;

public sealed partial class TcpClientNetwork
{
    public event Action<TcpClientNetwork, Package>? NewPackageReceived;

    public event Action<TcpClientNetwork>? Disconnected; 

    public bool IsConnected => _tcpClient.Connected;
    
    public IPEndPoint RemoteIpEndPoint { get; }
    
    private readonly PokemonServerOptions _options;
    private readonly ILogger _logger;
    
    private readonly TcpClient _tcpClient;

    private readonly Task _readingTask;
    private readonly Task _writingTask;
    
    private readonly ConcurrentQueue<Package> _packages = new();

    private DateTime _lastValidPackage = DateTime.Now;

    public TcpClientNetwork(PokemonServerContext context, TcpClient tcpClient)
    {
        _options = context.Options;
        _logger = context.Logger;
        _tcpClient = tcpClient;
        
        RemoteIpEndPoint = (_tcpClient.Client.RemoteEndPoint as IPEndPoint)!;
        
        _readingTask = Task.Factory.StartNew(RunReadingTask, TaskCreationOptions.LongRunning).Unwrap();
        _writingTask = Task.Factory.StartNew(RunWritingTask, TaskCreationOptions.LongRunning).Unwrap();
    }

    public void Disconnect()
    {
        _tcpClient.Close();
        Disconnected?.Invoke(this);
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
                var rawData = await streamReader.ReadLineAsync();
                if (rawData == null) break;

                PrintReceiveRawPackage(RemoteIpEndPoint.Address, rawData);

                var package = new Package(rawData);

                if (!package.IsValid)
                {
                    PrintInvalidPackageReceive(RemoteIpEndPoint.Address);
                }
                else
                {
                    _lastValidPackage = DateTime.Now;
                    NewPackageReceived?.Invoke(this, package);
                }
            }
            catch (OutOfMemoryException)
            {
                PrintOutOfMemory(RemoteIpEndPoint.Address);
            }
            catch (IOException)
            {
                PrintReadSocketIssue(RemoteIpEndPoint.Address);
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
                    var packageData = package.ToString(_options.ServerOptions.ProtocolVersion);
                    
                    PrintSendRawPackage(RemoteIpEndPoint.Address, packageData);
                
                    await streamWriter.WriteLineAsync(packageData);
                    await streamWriter.FlushAsync();
                    
                    package.TaskCompletionSource.SetResult();
                }
                catch (IOException exception)
                {
                    PrintWriteSocketIssue(RemoteIpEndPoint.Address);
                    package.TaskCompletionSource.SetException(exception);
                }
            }
        }
    }

    [LoggerMessage(0, LogLevel.Trace, "[{ipAddress}] Receive raw package data: {rawData}")]
    private partial void PrintReceiveRawPackage(IPAddress ipAddress, string rawData);
    
    [LoggerMessage(0, LogLevel.Trace, "[{ipAddress}] Send raw package data: {rawData}")]
    private partial void PrintSendRawPackage(IPAddress ipAddress, string rawData);

    [LoggerMessage(0, LogLevel.Debug, "[{ipAddress}] Unable to allocate buffer for the package data due to insufficient memory")]
    private partial void PrintOutOfMemory(IPAddress ipAddress);
    
    [LoggerMessage(0, LogLevel.Debug, "[{ipAddress}] Unable to read data from this network")]
    private partial void PrintReadSocketIssue(IPAddress ipAddress);
    
    [LoggerMessage(0, LogLevel.Debug, "[{ipAddress}] Unable to write data from this network")]
    private partial void PrintWriteSocketIssue(IPAddress ipAddress);
    
    [LoggerMessage(0, LogLevel.Debug, "[{ipAddress}] Invalid package received")]
    private partial void PrintInvalidPackageReceive(IPAddress ipAddress);
}