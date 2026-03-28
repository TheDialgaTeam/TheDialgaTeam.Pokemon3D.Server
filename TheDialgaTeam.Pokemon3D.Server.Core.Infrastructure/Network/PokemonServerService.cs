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
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Listener.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Upnp.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network;

public partial class PokemonServerService(
    ILogger<PokemonServerService> logger,
    IOptionsMonitor<ServerOptions> optionsMonitor,
    INatDeviceServiceFactory natDeviceServiceFactory,
    INetworkListenerFactory networkListenerFactory,
    NetworkClientContainer networkClientContainer
) : IPokemonServerService
{
    public IObservable<bool> IsActive => _isActiveSubject.AsObservable();
    public IObservable<bool> IsListening => _isListeningSubject.AsObservable();

    private bool _isActive;
    private readonly BehaviorSubject<bool> _isActiveSubject = new(false);

    private IPEndPoint _targetEndPoint = new(IPAddress.Any, 15124);

    private INatDeviceService? _natDeviceService;
    private INetworkListener? _networkListener;

    private readonly BehaviorSubject<bool> _isListeningSubject = new(false);

    private CompositeDisposable _disposable = new();

    [LoggerMessage(LogLevel.Information, "[Server] Found NAT device: {deviceEndPoint}")]
    private static partial void LogFoundNatDeviceDevice(ILogger<PokemonServerService> logger, IPEndPoint deviceEndPoint);

    [LoggerMessage(LogLevel.Information, "[Server] Server is listening on {ipEndPoint}.")]
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

        _disposable = new CompositeDisposable();

        _networkListener = networkListenerFactory.Create(_targetEndPoint);
        _networkListener.ObserveConnections
            .Do(client => LogServerNewIncomingConnection(logger, client.RemoteEndPoint))
            .Subscribe(networkClientContainer.AddNewConnection)
            .DisposeWith(_disposable);
        _networkListener.IsListening.Subscribe(_isListeningSubject).DisposeWith(_disposable);
        _networkListener.StartListening();

        logger.LogInformation("[Server] Server started.");

        _isActiveSubject.OnNext(true);
    }

    public async Task StopServerAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _isActive, false) is not true) return;

        logger.LogInformation("[Server] Stopping server...");

        _networkListener?.StopListening();
        _disposable.Dispose();

        if (_natDeviceService is not null)
        {
            await _natDeviceService.DestroyPortMappingAsync(_targetEndPoint.Port, cancellationToken).ConfigureAwait(false);
        }

        logger.LogInformation("[Server] Server stopped.");

        _isActiveSubject.OnNext(false);
    }
}