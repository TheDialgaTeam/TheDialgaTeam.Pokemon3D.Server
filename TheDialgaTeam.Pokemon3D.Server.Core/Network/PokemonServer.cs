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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Clients;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public sealed partial class PokemonServer : BackgroundService
{
    private readonly ILogger<PokemonServer> _logger;
    private readonly IPokemonServerOptions _options;
    private readonly HttpClient _httpClient;

    private TcpListener? _tcpListener;

    public PokemonServer(ILogger<PokemonServer> logger, IPokemonServerOptions options)
    {
        _logger = logger;
        _options = options;
        _httpClient = new HttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            PrintServerStarting();

            if (_options.NetworkOptions.UseUpnp)
            {
                await CreatePortMappingAsync(stoppingToken).ConfigureAwait(false);
            }

            _tcpListener = new TcpListener(_options.NetworkOptions.BindIpEndPoint);
            _tcpListener.Start();

            PrintServerStarted(_options.NetworkOptions.BindIpEndPoint);

            if (_options.ServerOptions.OfflineMode)
            {
                PrintServerOfflineMode();
            }

            _ = Task.Run(RunServerPortCheckingTask, stoppingToken);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync(stoppingToken).ConfigureAwait(false);
                    AddClient(new TcpClientNetwork(_logger, _options, tcpClient));
                }
            }
            catch (TaskCanceledException)
            {
                _tcpListener.Stop();
            }
            finally
            {
                PrintServerStopping();
            }
        }
        catch (SocketException exception)
        {
            PrintServerError(exception.SocketErrorCode, exception.Message);
        }
        catch (TaskCanceledException)
        {
        }
        finally
        {
            if (_options.NetworkOptions.UseUpnp)
            {
                await DestroyPortMappingAsync().ConfigureAwait(false);
            }

            PrintServerStopped();
        }
    }

    private async Task<IPAddress> GetPublicIpAddressAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_options.NetworkOptions.PublicIpAddress)) return IPAddress.Parse(_options.NetworkOptions.PublicIpAddress);

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);

        var externalIpAddress = await _httpClient.GetStringAsync("https://api.ipify.org", linkedCancellationTokenSource.Token).ConfigureAwait(false);
        return IPAddress.Parse(externalIpAddress);
    }

    private async Task RunServerPortCheckingTask()
    {
        var portToCheck = _options.NetworkOptions.BindIpEndPoint.Port;

        PrintRunningPortCheck(portToCheck);

        try
        {
            var publicIpAddress = await GetPublicIpAddressAsync().ConfigureAwait(false);

            try
            {
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(publicIpAddress, portToCheck, cancellationTokenSource.Token).ConfigureAwait(false);

                PrintPublicPortIsAvailable(portToCheck, new IPEndPoint(publicIpAddress, portToCheck));
            }
            catch
            {
                PrintPublicPortIsNotAvailable(portToCheck);
            }
        }
        catch
        {
            PrintUnableToGetPublicIpAddress(portToCheck);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "[Server] Starting Pokemon 3D Server.")]
    private partial void PrintServerStarting();

    [LoggerMessage(Level = LogLevel.Information, Message = "[Server] Started listening on {ipEndPoint} for new players.")]
    private partial void PrintServerStarted(IPEndPoint ipEndPoint);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Server] Server is allowing offline players")]
    private partial void PrintServerOfflineMode();

    [LoggerMessage(Level = LogLevel.Information, Message = "[Server] Checking port {port} is open...")]
    private partial void PrintRunningPortCheck(int port);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Server] Unable to get public ip address. Ensure that you have open the port {port} for external users to join.")]
    private partial void PrintUnableToGetPublicIpAddress(int port);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Server] Port {port} is opened. External users will be able to join via {ipEndpoint}.")]
    private partial void PrintPublicPortIsAvailable(int port, IPEndPoint ipEndPoint);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Server] Port {port} is not opened. External users will not be able to join.")]
    private partial void PrintPublicPortIsNotAvailable(int port);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Server] Stopping Pokemon 3D Server")]
    private partial void PrintServerStopping();

    [LoggerMessage(Level = LogLevel.Error, Message = "[Server] Error ({socketError}): {message}")]
    private partial void PrintServerError(SocketError socketError, string message);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Server] Stopped listening for new players")]
    private partial void PrintServerStopped();

    public override void Dispose()
    {
        base.Dispose();
        _httpClient.Dispose();
    }
}