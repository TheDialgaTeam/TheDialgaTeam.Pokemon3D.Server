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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

internal sealed partial class PokemonServerListener : BackgroundService, IPokemonServerListener
{
    private readonly ILogger<PokemonServerListener> _logger;
    private readonly IMediator _mediator;
    private readonly IPokemonServerOptions _options;
    private readonly INatDeviceUtility _natDeviceUtility;
    private readonly IPokemonServerClientFactory _pokemonServerClientFactory;

    private readonly HttpClient _httpClient = new();

    private TcpListener? _tcpListener;

    public PokemonServerListener(
        ILogger<PokemonServerListener> logger,
        IMediator mediator,
        IPokemonServerOptions options,
        INatDeviceUtility natDeviceUtility,
        IPokemonServerClientFactory pokemonServerClientFactory)
    {
        _logger = logger;
        _mediator = mediator;
        _options = options;
        _natDeviceUtility = natDeviceUtility;
        _pokemonServerClientFactory = pokemonServerClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var networkOptions = _options.NetworkOptions;
        var serverOptions = _options.ServerOptions;

        try
        {
            PrintServerStarting();

            if (networkOptions.UseUpnp)
            {
                await _natDeviceUtility.CreatePortMappingAsync(stoppingToken).ConfigureAwait(false);
            }

            stoppingToken.ThrowIfCancellationRequested();

            _tcpListener = new TcpListener(networkOptions.BindIpEndPoint);
            _tcpListener.Start();

            PrintServerStarted(networkOptions.BindIpEndPoint);

            if (serverOptions.OfflineMode)
            {
                PrintServerOfflineMode();
            }

            PrintServerPlayerCanJoinVia(string.Join(", ", serverOptions.GameModes));

            _ = Task.Run(() => RunServerPortCheckingTask(stoppingToken).GetAwaiter().GetResult(), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync(stoppingToken).ConfigureAwait(false);
                await _mediator.Publish(new Connected(_pokemonServerClientFactory.CreateTcpClientNetwork(tcpClient)), stoppingToken).ConfigureAwait(false);
            }
        }
        catch (SocketException exception)
        {
            PrintServerError(exception, exception.SocketErrorCode, exception.Message);
        }
        catch (TaskCanceledException)
        {
        }

        _tcpListener?.Stop();

        if (networkOptions.UseUpnp)
        {
            await _natDeviceUtility.DestroyPortMappingAsync().ConfigureAwait(false);
        }

        PrintServerStopped();
    }

    private async Task RunServerPortCheckingTask(CancellationToken cancellationToken)
    {
        var portToCheck = _options.NetworkOptions.BindIpEndPoint.Port;

        PrintRunningPortCheck(portToCheck);

        try
        {
            var publicIpAddress = IPAddress.Parse(await _httpClient.GetStringAsync("https://api.ipify.org", cancellationToken).ConfigureAwait(false));

            try
            {
                using var noPingCancellationTokenSource = new CancellationTokenSource(_options.ServerOptions.NoPingKickTime);
                using var globalCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(noPingCancellationTokenSource.Token, cancellationToken);

                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(publicIpAddress, portToCheck, globalCancellationTokenSource.Token).ConfigureAwait(false);

                await using var streamWriter = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8, tcpClient.SendBufferSize);
                streamWriter.AutoFlush = true;
                await streamWriter.WriteLineAsync(new ServerRequestPacket().ToRawPacket().ToRawPacketString().AsMemory(), globalCancellationTokenSource.Token).ConfigureAwait(false);

                using var streamReader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8, false, tcpClient.ReceiveBufferSize);
                var data = await streamReader.ReadLineAsync(globalCancellationTokenSource.Token).ConfigureAwait(false);

                if (RawPacket.TryParse(data, out var _))
                {
                    PrintPublicPortIsAvailable(portToCheck, new IPEndPoint(publicIpAddress, portToCheck));
                }
                else
                {
                    PrintPublicPortIsNotAvailable(portToCheck);
                }
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

    [LoggerMessage(LogLevel.Information, "[Server] Starting Pokemon 3D Server.")]
    private partial void PrintServerStarting();

    [LoggerMessage(LogLevel.Information, "[Server] Server started listening on {ipEndpoint}.")]
    private partial void PrintServerStarted(IPEndPoint ipEndPoint);

    [LoggerMessage(LogLevel.Information, "[Server] Players with offline profile can join the server.")]
    private partial void PrintServerOfflineMode();

    [LoggerMessage(LogLevel.Information, "[Server] Players can join with the following GameModes: {gameModes}.")]
    private partial void PrintServerPlayerCanJoinVia(string gameModes);

    [LoggerMessage(LogLevel.Information, "[Server] Checking port {port} is open...")]
    private partial void PrintRunningPortCheck(int port);

    [LoggerMessage(LogLevel.Warning, "[Server] Unable to get public IP Address. Ensure that you have open the port {port} for players to join.")]
    private partial void PrintUnableToGetPublicIpAddress(int port);

    [LoggerMessage(LogLevel.Information, "[Server] Port {port} is opened. Players will be able to join via {ipEndpoint} (Public).")]
    private partial void PrintPublicPortIsAvailable(int port, IPEndPoint ipEndPoint);

    [LoggerMessage(LogLevel.Information, "[Server] Port {port} is not opened. Players will not be able to join.")]
    private partial void PrintPublicPortIsNotAvailable(int port);

    [LoggerMessage(LogLevel.Error, "[Server] Error ({socketError}): {message}")]
    private partial void PrintServerError(Exception exception, SocketError socketError, string message);

    [LoggerMessage(LogLevel.Information, "[Server] Stopped listening for new players")]
    private partial void PrintServerStopped();

    public override void Dispose()
    {
        _httpClient.Dispose();
        base.Dispose();
    }
}