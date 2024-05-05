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
using Mono.Nat;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Formats;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Commands;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Utilities;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Commands;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

internal sealed partial class PokemonServerHandler(
    ILogger<PokemonServerHandler> logger,
    IPokemonServerOptions options,
    IStringLocalizer stringLocalizer,
    IMediator mediator,
    HttpClient httpClient,
    IPokemonServerClientFactory pokemonServerClientFactory) :
    ICommandHandler<StartServer>,
    ICommandHandler<StopServer>
{
    private readonly ILogger _logger = logger;
    
    private IPEndPoint _targetEndPoint = IPEndPoint.Parse(options.NetworkOptions.BindingInformation);
    private INatDevice? _natDevice;

    private CancellationTokenSource? _serverListenerCts;
    
    public async ValueTask<Unit> Handle(StartServer command, CancellationToken cancellationToken)
    {
        await StartServerTask(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }

    public ValueTask<Unit> Handle(StopServer command, CancellationToken cancellationToken)
    {
        _serverListenerCts?.Cancel();
        return Unit.ValueTask;
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
        
        var serverListenerTask = ServerListenerTask(cancellationToken);

        if (serverListenerTask.IsFaulted)
        {
            throw serverListenerTask.Exception;
        }
    }

    private async Task CreatePortMappingTask(CancellationToken cancellationToken)
    {
        var natDiscoveryTime = TimeSpan.FromSeconds(options.NetworkOptions.UpnpDiscoveryTime);
        
        using var upnpMaxDiscoveryTimeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        upnpMaxDiscoveryTimeCts.CancelAfter(natDiscoveryTime);
            
        PrintNatInformation(stringLocalizer[s => s.ConsoleMessageFormat.NatSearchForUpnpDevice, natDiscoveryTime.TotalSeconds]);
            
        _natDevice = await NatDeviceUtility.DiscoverNatDeviceAsync(upnpMaxDiscoveryTimeCts.Token).ConfigureAwait(false);
        if (_natDevice == null) return;
        
        PrintNatInformation(stringLocalizer[s => s.ConsoleMessageFormat.NatFoundUpnpDevice, _natDevice.DeviceEndpoint.Address]);

        if (cancellationToken.IsCancellationRequested) return;
            
        var natDevicesMapping = await NatDeviceUtility.CreatePortMappingAsync(_natDevice, _targetEndPoint).ConfigureAwait(false);

        if (natDevicesMapping != null && natDevicesMapping.PrivatePort == _targetEndPoint.Port && natDevicesMapping.PublicPort == _targetEndPoint.Port)
        {
            PrintNatInformation(stringLocalizer[s => s.ConsoleMessageFormat.NatCreatedUpnpDeviceMapping, _natDevice.DeviceEndpoint.Address]);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            await NatDeviceUtility.DestroyPortMappingAsync(_natDevice, _targetEndPoint).ConfigureAwait(false);
        }
    }
    
    private async Task ServerListenerTask(CancellationToken cancellationToken)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested) return;
            
            using var tcpListener = new TcpListener(_targetEndPoint);
            tcpListener.Start();

            PrintServerInformation(stringLocalizer[s => s.ConsoleMessageFormat.ServerStartedListening, _targetEndPoint]);

            if (options.ServerOptions.AllowOfflinePlayer)
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
            
            await mediator.Send(new StartLocalWorld(), cancellationToken).ConfigureAwait(false);
            _ = Task.Run(() => ServerPortCheckingTask(cancellationToken), cancellationToken);

            _serverListenerCts = new CancellationTokenSource();
            var serverListenerToken = _serverListenerCts.Token;

            while (!serverListenerToken.IsCancellationRequested)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync(serverListenerToken).ConfigureAwait(false);
                await mediator.Publish(new ClientConnected(pokemonServerClientFactory.CreateTcpClientNetwork(tcpClient)), serverListenerToken).ConfigureAwait(false);
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
                if (_natDevice != null)
                {
                    await NatDeviceUtility.DestroyPortMappingAsync(_natDevice, _targetEndPoint).ConfigureAwait(false);
                }
            }
            
            PrintServerInformation(stringLocalizer[s => s.ConsoleMessageFormat.ServerStoppedListening]);
        }
    }

    private async Task ServerPortCheckingTask(CancellationToken cancellationToken)
    {
        PrintServerInformation(stringLocalizer[token => token.ConsoleMessageFormat.ServerRunningPortCheck, _targetEndPoint.Port]);
        
        try
        {
            var publicIpAddress = IPAddress.Parse(await httpClient.GetStringAsync("https://api.ipify.org", cancellationToken).ConfigureAwait(false));
                
            try
            {
                using var connectionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                connectionCts.CancelAfter(TimeSpan.FromSeconds(options.ServerOptions.NoPingKickTime));
                
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(publicIpAddress, _targetEndPoint.Port, connectionCts.Token).ConfigureAwait(false);

                await using var streamWriter = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8, tcpClient.SendBufferSize);
                streamWriter.AutoFlush = true;
                await streamWriter.WriteLineAsync(new ServerDataRequestPacket("r").ToClientRawPacket().ToRawPacketString().AsMemory(), connectionCts.Token).ConfigureAwait(false);

                using var streamReader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8, false, tcpClient.ReceiveBufferSize);
                var data = await streamReader.ReadLineAsync(connectionCts.Token).ConfigureAwait(false);

                PrintServerInformation(RawPacket.TryParse(data, out var _) ? stringLocalizer[token => token.ConsoleMessageFormat.ServerPortIsOpened, _targetEndPoint.Port, new IPEndPoint(publicIpAddress, _targetEndPoint.Port)] : stringLocalizer[token => token.ConsoleMessageFormat.ServerPortIsClosed, _targetEndPoint.Port]);
            }
            catch
            {
                PrintServerInformation(stringLocalizer[token => token.ConsoleMessageFormat.ServerPortIsClosed, _targetEndPoint.Port]);
            }
        }
        catch
        {
            PrintServerInformation(stringLocalizer[token => token.ConsoleMessageFormat.ServerPortCheckFailed, _targetEndPoint.Port]);
        }
    }
    
    [LoggerMessage(LogLevel.Information, "[NAT] {Message}")]
    private partial void PrintNatInformation(string message);
    
    [LoggerMessage(LogLevel.Information, "[Server] {Message}")]
    private partial void PrintServerInformation(string message);
    
    [LoggerMessage(LogLevel.Error, "[Server] {Message}")]
    private partial void PrintServerError(Exception exception, string message);
}