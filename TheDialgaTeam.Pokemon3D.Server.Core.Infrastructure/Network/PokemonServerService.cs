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

using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Network.Client;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Listener;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Upnp;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network;

public partial class PokemonServerService(
    ILogger<PokemonServerService> logger,
    IOptionsMonitor<ServerOptions> optionsMonitor,
    INatDeviceFactory natDeviceFactory,
    INetworkListener networkListener
) : IPokemonServerService
{
    public IObservable<INetworkClient> ObserveConnections => _networkClientSubject.AsObservable();
    
    private IPEndPoint _targetEndPoint = new(IPAddress.Any, 15124);
    private INatDeviceService? _natDeviceService;
    private IDisposable? _disposables;

    private bool _isActive;
    private readonly Subject<INetworkClient> _networkClientSubject = new();

    [LoggerMessage(LogLevel.Information, "Found NAT device: {deviceEndPoint}")]
    private static partial void LogFoundNatDeviceDevice(ILogger<PokemonServerService> logger, IPEndPoint deviceEndPoint);

    [LoggerMessage(LogLevel.Information, "Server started listening at: {ipEndPoint}")]
    private static partial void LogServerStartedListeningAtIpEndpoint(ILogger<PokemonServerService> logger, IPEndPoint ipEndPoint);

    public async Task StartServerAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _isActive, true) is not false) return;
        
        logger.LogInformation("Starting Pokemon 3D Server");

        var options = optionsMonitor.CurrentValue;

        _targetEndPoint = IPEndPoint.Parse(options.BindingInformation);

        if (options.UseUpnp)
        {
            using var upnpCTS = new CancellationTokenSource(TimeSpan.FromSeconds(options.UpnpDiscoveryTime));
            using var unionCTS = CancellationTokenSource.CreateLinkedTokenSource(upnpCTS.Token, cancellationToken);

            try
            {
                _natDeviceService = await natDeviceFactory.GetNatDeviceServiceAsync(unionCTS.Token).ConfigureAwait(false);
                LogFoundNatDeviceDevice(logger, _natDeviceService.DeviceEndpoint);

                try
                {
                    _targetEndPoint.Port = await _natDeviceService.CreatePortMappingAsync(_targetEndPoint.Port, cancellationToken).ConfigureAwait(false);
                    logger.LogInformation("Created port mapping.");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Port mapping failed. You will need to open the port yourself in order for people to connect to this server.");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No NAT device found.");
            }
        }

        _disposables = networkListener.ObserveConnections(_targetEndPoint).Subscribe(_networkClientSubject);
    }

    public async Task StopServerAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _isActive, false) is not true) return;
        
        _disposables?.Dispose();

        if (_natDeviceService is not null)
        {
            await _natDeviceService.DestroyPortMappingAsync(_targetEndPoint.Port, cancellationToken).ConfigureAwait(false);
        }
        
        logger.LogInformation("Stopped Pokemon 3D Server");
    }
}