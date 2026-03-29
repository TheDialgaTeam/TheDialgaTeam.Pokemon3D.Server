// Pokemon 3D Server Client
// Copyright (C) 2026 Yong Jian Ming
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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Client;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Listener;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Upnp.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network;

public partial class PokemonServerService(
    ILogger<PokemonServerService> logger,
    IOptionsMonitor<ServerOptions> optionsMonitor,
    INatDeviceServiceFactory natDeviceServiceFactory,
    NetworkClientContainer networkClientContainer
) : IPokemonServerService
{
    public IObservable<bool> IsActive => _isActiveSubject.AsObservable();

    private bool _isActive;
    private readonly BehaviorSubject<bool> _isActiveSubject = new(false);

    private IPEndPoint _targetEndPoint = new(IPAddress.Any, 15124);

    private INatDeviceService? _natDeviceService;
    
    [AllowNull]
    private IDisposable _networkListener;

    [LoggerMessage(LogLevel.Information, "[Server] Found NAT device: {DeviceEndPoint}")]
    private static partial void LogFoundNatDeviceDevice(ILogger<PokemonServerService> logger, IPEndPoint deviceEndPoint);

    [LoggerMessage(LogLevel.Information, "[Server] Server is listening on {IpEndPoint}.")]
    private static partial void LogServerIsListeningOn(ILogger<PokemonServerService> logger, IPEndPoint ipEndPoint);

    [LoggerMessage(LogLevel.Trace, "[Server] New incoming connection: {RemoteEndPoint}")]
    private static partial void LogServerNewIncomingConnection(ILogger<PokemonServerService> logger, IPEndPoint RemoteEndPoint);

    public async Task StartServerAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _isActive, true) is not false) return;

        logger.LogInformation("[Server] Starting server...");

        var options = optionsMonitor.CurrentValue;

        _targetEndPoint = IPEndPoint.Parse(options.BindingInformation);

        if (options.UseUpnp)
        {
            using var upnpCTS = new CancellationTokenSource(TimeSpan.FromSeconds(options.UpnpDiscoveryTime));
            using var unionCTS = CancellationTokenSource.CreateLinkedTokenSource(upnpCTS.Token, cancellationToken);

            try
            {
                logger.LogInformation("[Server] Searching for NAT devices...");
                _natDeviceService = await natDeviceServiceFactory.GetAsync(unionCTS.Token).ConfigureAwait(false);
                LogFoundNatDeviceDevice(logger, _natDeviceService.DeviceEndpoint);

                try
                {
                    _targetEndPoint.Port = await _natDeviceService.CreatePortMappingAsync(_targetEndPoint.Port, cancellationToken).ConfigureAwait(false);
                    logger.LogInformation("[Server] Created port mapping.");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "[Server] Port mapping failed. You will need to open the port yourself in order for people to connect to this server.");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[Server] No NAT device found.");
            }
        }
        
        _networkListener = TcpNetworkListener.Create(_targetEndPoint)
            .Do(client => LogServerNewIncomingConnection(logger, client.RemoteEndPoint))
            .Catch<INetworkClient, Exception>(exception =>
            {
                logger.LogError(exception, "[Server] {Message}", exception.Message);
                return Observable.Empty<INetworkClient>();
            })
            .Subscribe(networkClientContainer.AddNewConnection);
        
        logger.LogInformation("[Server] Server started.");

        _isActiveSubject.OnNext(true);
    }

    public async Task StopServerAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _isActive, false) is not true) return;

        logger.LogInformation("[Server] Stopping server...");

        _networkListener.Dispose();

        if (_natDeviceService is not null)
        {
            await _natDeviceService.DestroyPortMappingAsync(_targetEndPoint.Port, cancellationToken).ConfigureAwait(false);
        }

        logger.LogInformation("[Server] Server stopped.");

        _isActiveSubject.OnNext(false);
    }
}