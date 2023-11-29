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
using TheDialgaTeam.Pokemon3D.Server.Core.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Commands;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public sealed class PokemonServerListener : BackgroundService, ICommandHandler<StartServer>, ICommandHandler<StopServer>
{
    private readonly ILogger _logger;
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

    public async ValueTask<Unit> Handle(StartServer command, CancellationToken cancellationToken)
    {
        await StartAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }

    public async ValueTask<Unit> Handle(StopServer command, CancellationToken cancellationToken)
    {
        await StopAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var networkOptions = _options.NetworkOptions;
        var serverOptions = _options.ServerOptions;
        
        try
        {
            Logger.PrintServerStarting(_logger);

            if (networkOptions.UseUpnp)
            {
                await _natDeviceUtility.CreatePortMappingAsync(stoppingToken).ConfigureAwait(false);
            }

            stoppingToken.ThrowIfCancellationRequested();

            _tcpListener = new TcpListener(networkOptions.BindIpEndPoint);
            _tcpListener.Start();

            Logger.PrintServerStarted(_logger, networkOptions.BindIpEndPoint);

            if (serverOptions.OfflineMode)
            {
                Logger.PrintServerOfflineMode(_logger);
            }

            if (serverOptions is { AllowAnyGameModes: true, BlacklistedGameModes.Length: 0 })
            {
                Logger.PrintServerPlayerCanJoinWithAny(_logger);
            }

            if (serverOptions is { AllowAnyGameModes: true, BlacklistedGameModes.Length: > 0 })
            {
                Logger.PrintServerPlayerCanJoinWithAnyExcept(_logger, string.Join(", ", serverOptions.BlacklistedGameModes));
            }

            if (!serverOptions.AllowAnyGameModes)
            {
                Logger.PrintServerPlayerCanJoinWith(_logger, string.Join(", ", serverOptions.WhitelistedGameModes));
            }

            _ = Task.Run(() => RunServerPortCheckingTask(networkOptions, serverOptions, stoppingToken), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync(stoppingToken).ConfigureAwait(false);
                await _mediator.Publish(new Connected(_pokemonServerClientFactory.CreateTcpClientNetwork(tcpClient)), stoppingToken).ConfigureAwait(false);
            }
        }
        catch (SocketException exception)
        {
            Logger.PrintServerError(_logger, exception, exception.SocketErrorCode, exception.Message);
        }
        catch (OperationCanceledException)
        {
        }

        _tcpListener?.Stop();

        if (networkOptions.UseUpnp)
        {
            await _natDeviceUtility.DestroyPortMappingAsync().ConfigureAwait(false);
        }

        Logger.PrintServerStopped(_logger);
    }

    private async Task RunServerPortCheckingTask(NetworkOptions networkOptions, ServerOptions serverOptions, CancellationToken cancellationToken)
    {
        var portToCheck = networkOptions.BindIpEndPoint.Port;

        Logger.PrintRunningPortCheck(_logger, portToCheck);

        try
        {
            var publicIpAddress = IPAddress.Parse(await _httpClient.GetStringAsync("https://api.ipify.org", cancellationToken).ConfigureAwait(false));

            try
            {
                using var noPingCancellationTokenSource = new CancellationTokenSource(serverOptions.NoPingKickTime);
                using var globalCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(noPingCancellationTokenSource.Token, cancellationToken);

                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(publicIpAddress, portToCheck, globalCancellationTokenSource.Token).ConfigureAwait(false);

                await using var streamWriter = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8, tcpClient.SendBufferSize);
                streamWriter.AutoFlush = true;
                await streamWriter.WriteLineAsync(new ServerRequestPacket("r").ToRawPacket().ToRawPacketString().AsMemory(), globalCancellationTokenSource.Token).ConfigureAwait(false);

                using var streamReader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8, false, tcpClient.ReceiveBufferSize);
                var data = await streamReader.ReadLineAsync(globalCancellationTokenSource.Token).ConfigureAwait(false);

                if (RawPacket.TryParse(data, out var _))
                {
                    Logger.PrintPublicPortIsAvailable(_logger, portToCheck, new IPEndPoint(publicIpAddress, portToCheck));
                }
                else
                {
                    Logger.PrintPublicPortIsNotAvailable(_logger, portToCheck);
                }
            }
            catch
            {
                Logger.PrintPublicPortIsNotAvailable(_logger, portToCheck);
            }
        }
        catch
        {
            Logger.PrintUnableToGetPublicIpAddress(_logger, portToCheck);
        }
    }

    public override void Dispose()
    {
        _tcpListener?.Dispose();
        _httpClient.Dispose();
        base.Dispose();
    }
}