﻿// Pokemon 3D Server Client
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
using Mono.Nat;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Formats;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Commands;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Utilities;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Commands;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public sealed partial class PokemonServerHandler(
    ILogger<PokemonServerHandler> logger,
    IPokemonServerOptions options,
    IStringLocalizer stringLocalizer,
    IMediator mediator,
    IPokemonServerClientFactory pokemonServerClientFactory) :
    ICommandHandler<StartServer>,
    ICommandHandler<StopServer>
{
    private readonly ILogger _logger = logger;
    
    private int _serverActiveStatus;
    
    private IPEndPoint _targetEndPoint = IPEndPoint.Parse(options.NetworkOptions.BindingInformation);
    private INatDevice[] _natDevices = Array.Empty<INatDevice>();

    private CancellationTokenSource? _serverListenerCts;
    
    private readonly HttpClient _httpClient = new();
    
    public async ValueTask<Unit> Handle(StartServer command, CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _serverActiveStatus, 1) == 1) return Unit.Value;
        await StartServerTask(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }

    public async ValueTask<Unit> Handle(StopServer command, CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _serverActiveStatus, 0) == 0) return Unit.Value;
        
        return Unit.Value;
    }

    private async Task StartServerTask(CancellationToken cancellationToken)
    {
        PrintServerInformation(stringLocalizer[s => s.ConsoleMessageFormat.ServerIsStarting]);

        _targetEndPoint = IPEndPoint.Parse(options.NetworkOptions.BindingInformation);

        if (options.NetworkOptions.UseUpnp)
        {
            await CreatePortMappingTask(cancellationToken).ConfigureAwait(false);
        }

        if (cancellationToken.IsCancellationRequested) return;
        
        var serverListenerTask = ServerListenerTask();

        if (serverListenerTask.IsFaulted)
        {
            throw serverListenerTask.Exception;
        }
    }

    private async Task CreatePortMappingTask(CancellationToken cancellationToken)
    {
        var natDiscoveryTime = TimeSpan.FromSeconds(options.NetworkOptions.UpnpDiscoveryTime);
            
        using var natDeviceDiscoveryCts = new CancellationTokenSource(natDiscoveryTime);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(natDeviceDiscoveryCts.Token, cancellationToken);
            
        PrintNatInformation(stringLocalizer[s => s.ConsoleMessageFormat.NatSearchForUpnpDevices, natDiscoveryTime.TotalSeconds]);
            
        _natDevices = await NatDeviceUtility.DiscoverNatDevicesAsync(linkedCts.Token).ConfigureAwait(false);
            
        PrintNatInformation(stringLocalizer[s => s.ConsoleMessageFormat.NatFoundUpnpDevices, _natDevices.Length]);

        if (cancellationToken.IsCancellationRequested) return;
            
        var natDevicesMappings = await NatDeviceUtility.CreatePortMappingAsync(_natDevices, _targetEndPoint).ConfigureAwait(false);

        foreach (var (natDevice, mapping) in natDevicesMappings)
        {
            if (cancellationToken.IsCancellationRequested) break;
            PrintNatInformation(stringLocalizer[s => s.ConsoleMessageFormat.NatCreatedUpnpDeviceMapping, natDevice.DeviceEndpoint.Address]);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            await NatDeviceUtility.DestroyPortMappingAsync(_natDevices, _targetEndPoint).ConfigureAwait(false);
        }
    }
    
    private async Task ServerListenerTask()
    {
        try
        {
            using var tcpListener = new TcpListener(_targetEndPoint);
            tcpListener.Start();

            PrintServerInformation(stringLocalizer[s => s.ConsoleMessageFormat.ServerStartedListening, _targetEndPoint]);

            if (options.ServerOptions.OfflineMode)
            {
                PrintServerInformation(stringLocalizer[s => s.ConsoleMessageFormat.ServerAllowOfflineProfile]);
            }

            switch (options.ServerOptions.AllowAnyGameModes)
            {
                case true when options.ServerOptions.BlacklistedGameModes.Length == 0:
                    PrintServerInformation(stringLocalizer[s => s.ConsoleMessageFormat.ServerAllowAnyGameModes]);
                    break;

                case true when options.ServerOptions.BlacklistedGameModes.Length > 0:
                    PrintServerInformation(stringLocalizer[s => s.ConsoleMessageFormat.ServerAllowAnyGameModesExcept, new ArrayStringFormat<string>(options.ServerOptions.BlacklistedGameModes)]);
                    break;

                default:
                    PrintServerInformation(stringLocalizer[s => s.ConsoleMessageFormat.ServerAllowOnlyGameModes, new ArrayStringFormat<string>(options.ServerOptions.WhitelistedGameModes)]);
                    break;
            }

            await mediator.Send(new StartGlobalWorld()).ConfigureAwait(false);

            _ = ServerPortCheckingTask(default);

            _serverListenerCts = new CancellationTokenSource();
            var cancellationToken = _serverListenerCts.Token;

            while (!cancellationToken.IsCancellationRequested)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                await mediator.Publish(new ClientConnected(pokemonServerClientFactory.CreateTcpClientNetwork(tcpClient)), cancellationToken).ConfigureAwait(false);
            }
        }
        catch (SocketException socketException)
        {
            PrintServerError(socketException, stringLocalizer[s => s.ConsoleMessageFormat.ServerBindingError, _targetEndPoint]);
            throw;
        }
        finally
        {
            if (options.NetworkOptions.UseUpnp)
            {
                await NatDeviceUtility.DestroyPortMappingAsync(_natDevices, _targetEndPoint).ConfigureAwait(false);
            }
            
            PrintServerInformation(stringLocalizer[s => s.ConsoleMessageFormat.ServerStoppedListening]);
        }
    }

    private async Task ServerPortCheckingTask(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Message}", stringLocalizer[token => token.ConsoleMessageFormat.ServerRunningPortCheck, _targetEndPoint.Port]);
        
        try
        {
            var publicIpAddress = IPAddress.Parse(await _httpClient.GetStringAsync("https://api.ipify.org", cancellationToken).ConfigureAwait(false));

            try
            {
                using var noPingCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(options.ServerOptions.NoPingKickTime));
                using var globalCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(noPingCancellationTokenSource.Token, cancellationToken);

                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(publicIpAddress, _targetEndPoint.Port, globalCancellationTokenSource.Token).ConfigureAwait(false);

                await using var streamWriter = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8, tcpClient.SendBufferSize);
                streamWriter.AutoFlush = true;
                await streamWriter.WriteLineAsync(new ServerRequestPacket("r").ToRawPacket().ToRawPacketString().AsMemory(), globalCancellationTokenSource.Token).ConfigureAwait(false);

                using var streamReader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8, false, tcpClient.ReceiveBufferSize);
                var data = await streamReader.ReadLineAsync(globalCancellationTokenSource.Token).ConfigureAwait(false);

                _logger.LogInformation("{Message}", RawPacket.TryParse(data, out var _) ? stringLocalizer[token => token.ConsoleMessageFormat.ServerPortIsOpened, _targetEndPoint.Port, new IPEndPoint(publicIpAddress, _targetEndPoint.Port)] : stringLocalizer[token => token.ConsoleMessageFormat.ServerPortIsClosed, _targetEndPoint.Port]);
            }
            catch
            {
                _logger.LogInformation("{Message}", stringLocalizer[token => token.ConsoleMessageFormat.ServerPortIsClosed, _targetEndPoint.Port]);
            }
        }
        catch
        {
            _logger.LogInformation("{Message}", stringLocalizer[token => token.ConsoleMessageFormat.ServerPortCheckFailed, _targetEndPoint.Port]);
        }
    }
    
    [LoggerMessage(LogLevel.Information, "[NAT] {Message}")]
    private partial void PrintNatInformation(string message);
    
    [LoggerMessage(LogLevel.Information, "[Server] {Message}")]
    private partial void PrintServerInformation(string message);
    
    [LoggerMessage(LogLevel.Error, "[Server] {Message}")]
    private partial void PrintServerError(Exception exception, string message);
}