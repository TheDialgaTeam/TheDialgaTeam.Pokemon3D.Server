using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Client;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Nat;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public sealed partial class PokemonServer
{
    public PokemonServerOptions Options { get; }

    internal ILogger Logger => _logger;

    private readonly ILogger<PokemonServer> _logger;
    private readonly HttpClient _httpClient;
    private readonly NatDeviceListener _natDeviceListener;

    private int _isActive;
    private CancellationTokenSource? _cancellationTokenSource;

    private Task? _serverListenerTask;
    private TcpListener? _tcpListener;

    private readonly List<TcpClientNetwork> _tcpClientNetworks = new();
    private readonly object _clientLock = new();

    public PokemonServer(ILogger<PokemonServer> logger, PokemonServerOptions options, HttpClient httpClient, NatDeviceListener natDeviceListener)
    {
        Options = options;
        _logger = logger;
        _httpClient = httpClient;
        _natDeviceListener = natDeviceListener;
    }

    public async Task StartAsync()
    {
        if (Interlocked.CompareExchange(ref _isActive, 1, 0) == 1) return;

        PrintServerStarting();

        _cancellationTokenSource = new CancellationTokenSource();
        
        if (Options.NetworkOptions.UseUpnp)
        {
            await _natDeviceListener.StartAsync().ConfigureAwait(false);
        }

        _serverListenerTask = Task.Factory.StartNew(RunServerListenerTask, TaskCreationOptions.LongRunning).Unwrap();
    }

    public async Task StopAsync()
    {
        if (Interlocked.CompareExchange(ref _isActive, 0, 1) == 0) return;

        PrintServerStopping();

        _cancellationTokenSource?.Cancel();
        
        if (Options.NetworkOptions.UseUpnp)
        {
            await _natDeviceListener.StopAsync().ConfigureAwait(false);
        }

        if (_serverListenerTask != null)
        {
            await _serverListenerTask.ConfigureAwait(false);
        }

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    private async Task RunServerListenerTask()
    {
        Debug.Assert(_cancellationTokenSource != null, nameof(_cancellationTokenSource) + " != null");
        
        try
        {
            _tcpListener = new TcpListener(Options.NetworkOptions.BindIpEndPoint);
            _tcpListener.Start();

            PrintServerStarted(Options.NetworkOptions.BindIpEndPoint);

            if (Options.ServerOptions.OfflineMode)
            {
                PrintServerOfflineMode();
            }
            
            var cancellationToken = _cancellationTokenSource.Token;
            _ = Task.Run(RunServerPortCheckingTask, cancellationToken);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    AddClient(await _tcpListener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false));
                }
            }
            catch (OperationCanceledException)
            {
                _tcpListener.Stop();
            }
            finally
            {
                PrintServerStopped();
            }
        }
        catch (SocketException exception)
        {
            PrintServerError(exception.SocketErrorCode, exception.Message);
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private async Task RunServerPortCheckingTask()
    {
        async Task<bool> IsExternalPortOpenAsync()
        {
            try
            {
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var externalIpAddress = await _httpClient.GetStringAsync("https://api.ipify.org", cancellationTokenSource.Token).ConfigureAwait(false);

                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(externalIpAddress, Options.NetworkOptions.BindIpEndPoint.Port, cancellationTokenSource.Token).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        PrintRunningPortCheck(Options.NetworkOptions.BindIpEndPoint.Port);

        if (await IsExternalPortOpenAsync().ConfigureAwait(false))
        {
            PrintPublicPortIsAvailable(Options.NetworkOptions.BindIpEndPoint.Port);
        }
        else
        {
            PrintPublicPortIsNotAvailable(Options.NetworkOptions.BindIpEndPoint.Port);
        }
    }

    private void AddClient(TcpClient tcpClient)
    {
        lock (_clientLock)
        {
            _tcpClientNetworks.Add(new TcpClientNetwork(new PokemonServerContext(this), tcpClient));
        }
    }

    [LoggerMessage(0, LogLevel.Information, "[Server] Starting Pokemon 3D Server")]
    private partial void PrintServerStarting();

    [LoggerMessage(0, LogLevel.Information, "[Server] Stopping Pokemon 3D Server")]
    private partial void PrintServerStopping();

    [LoggerMessage(0, LogLevel.Information, "[Server] Started listening on {ipEndPoint} for new players")]
    private partial void PrintServerStarted(IPEndPoint ipEndPoint);
    
    [LoggerMessage(0, LogLevel.Information, "[Server] Server is allowing offline players")]
    private partial void PrintServerOfflineMode();

    [LoggerMessage(0, LogLevel.Information, "[Server] Stopped listening for new players")]
    private partial void PrintServerStopped();

    [LoggerMessage(0, LogLevel.Error, "[Server] Error ({socketError}): {message}")]
    private partial void PrintServerError(SocketError socketError, string message);

    [LoggerMessage(0, LogLevel.Information, "[Server] Checking port {port} is open...")]
    private partial void PrintRunningPortCheck(int port);

    [LoggerMessage(0, LogLevel.Information, "[Server] Port {port} is opened. External users will be able to join.")]
    private partial void PrintPublicPortIsAvailable(int port);

    [LoggerMessage(0, LogLevel.Information, "[Server] Port {port} is not opened. External users will not be able to join.")]
    private partial void PrintPublicPortIsNotAvailable(int port);
}