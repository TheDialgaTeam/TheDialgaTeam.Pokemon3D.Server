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

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Clients;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Queries;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public interface IPokemonServer : IHostedService
{
}

internal sealed partial class PokemonServer : BackgroundService, IPokemonServer
{
    private readonly ILogger<PokemonServer> _logger;
    private readonly IMediator _mediator;
    private readonly TcpClientNetworkFactory _tcpClientNetworkFactory;
    private readonly HttpClient _httpClient;

    private TcpListener? _tcpListener;

    public PokemonServer(ILogger<PokemonServer> logger, IMediator mediator, TcpClientNetworkFactory tcpClientNetworkFactory)
    {
        _logger = logger;
        _mediator = mediator;
        _tcpClientNetworkFactory = tcpClientNetworkFactory;
        _httpClient = new HttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) });
    }

    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var networkOptions = await _mediator.SendAsync(GetNetworkOptions.Empty, stoppingToken).ConfigureAwait(false);
        var serverOptions = await _mediator.SendAsync(GetServerOptions.Empty, stoppingToken).ConfigureAwait(false);

        try
        {
            PrintServerStarting();
            
            if (networkOptions.UseUpnp)
            {
                await CreatePortMappingAsync(networkOptions.BindIpEndPoint, stoppingToken).ConfigureAwait(false);
            }

            _tcpListener = new TcpListener(networkOptions.BindIpEndPoint);
            _tcpListener.Start();

            PrintServerStarted(networkOptions.BindIpEndPoint);

            if (serverOptions.OfflineMode)
            {
                PrintServerOfflineMode();
            }

            _ = Task.Run(RunServerPortCheckingTask, stoppingToken);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync(stoppingToken).ConfigureAwait(false);
                    AddClient(_tcpClientNetworkFactory.CreateTcpClientNetwork(tcpClient));
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
            if (networkOptions.UseUpnp)
            {
                await DestroyPortMappingAsync(networkOptions.BindIpEndPoint).ConfigureAwait(false);
            }

            PrintServerStopped();
        }
    }
    
    private async Task<IPAddress> GetPublicIpAddressAsync(CancellationToken cancellationToken = default)
    {
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);

        var externalIpAddress = await _httpClient.GetStringAsync("https://api.ipify.org", linkedCancellationTokenSource.Token).ConfigureAwait(false);
        return IPAddress.Parse(externalIpAddress);
    }

    [RequiresDynamicCode("Calls TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces.IMediator.SendAsync<TResponse>(IRequest<TResponse>, CancellationToken)")]
    private async Task RunServerPortCheckingTask()
    {
        var networkOptions = await _mediator.SendAsync(GetNetworkOptions.Empty).ConfigureAwait(false);
        var portToCheck = networkOptions.BindIpEndPoint.Port;

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
        _httpClient.Dispose();
        base.Dispose();
    }
}