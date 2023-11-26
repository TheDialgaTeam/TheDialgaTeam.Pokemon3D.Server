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
using Microsoft.Extensions.Logging;
using Mono.Nat;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

internal sealed partial class NatDeviceUtility : INatDeviceUtility
{
    private readonly ILogger<NatDeviceUtility> _logger;
    private readonly IPokemonServerOptions _options;

    private INatDevice[] _natDevices = Array.Empty<INatDevice>();
    private IPEndPoint _targetEndPoint;

    public NatDeviceUtility(ILogger<NatDeviceUtility> logger, IPokemonServerOptions options)
    {
        _logger = logger;
        _options = options;
        _targetEndPoint = _options.NetworkOptions.BindIpEndPoint;
    }

    private static async Task<INatDevice[]> DiscoverNatDevicesAsync(CancellationToken cancellationToken)
    {
        var devices = new List<INatDevice>();

        try
        {
            NatUtility.DeviceFound += NatUtilityOnDeviceFound;
            NatUtility.StartDiscovery(NatProtocol.Upnp);

            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token);

            await Task.Delay(-1, linkedCancellationTokenSource.Token).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            NatUtility.DeviceFound -= NatUtilityOnDeviceFound;
            NatUtility.StopDiscovery();
        }

        return devices.ToArray();

        void NatUtilityOnDeviceFound(object? sender, DeviceEventArgs e)
        {
            devices.Add(e.Device);
        }
    }

    public async Task CreatePortMappingAsync(CancellationToken cancellationToken = default)
    {
        PrintNatSearchForUpnpDevices();
        
        _natDevices = await DiscoverNatDevicesAsync(cancellationToken).ConfigureAwait(false);
        
        PrintNatFoundUpnpDevices(_natDevices.Length);

        _targetEndPoint = _options.NetworkOptions.BindIpEndPoint;

        foreach (var natDevice in _natDevices)
        {
            if (!_targetEndPoint.Address.Equals(IPAddress.Any) && !_targetEndPoint.Address.Equals(natDevice.DeviceEndpoint.Address)) continue;

            var createMapping = true;

            try
            {
                var mapping = await natDevice.GetSpecificMappingAsync(Protocol.Tcp, _targetEndPoint.Port).ConfigureAwait(false);

                if (mapping.IsExpired())
                {
                    await natDevice.DeletePortMapAsync(mapping).ConfigureAwait(false);
                }
                else
                {
                    createMapping = false;
                }
            }
            catch
            {
                createMapping = true;
            }

            if (!createMapping) continue;

            await natDevice.CreatePortMapAsync(new Mapping(Protocol.Tcp, _targetEndPoint.Port, _targetEndPoint.Port)).ConfigureAwait(false);
            
            PrintNatCreatedUpnpDeviceMapping(natDevice.DeviceEndpoint.Address);
        }
    }

    public async Task DestroyPortMappingAsync()
    {
        foreach (var natDevice in _natDevices)
        {
            if (!_targetEndPoint.Address.Equals(IPAddress.Any) && !_targetEndPoint.Address.Equals(natDevice.DeviceEndpoint.Address)) continue;

            try
            {
                var mapping = await natDevice.GetSpecificMappingAsync(Protocol.Tcp, _targetEndPoint.Port).ConfigureAwait(false);
                await natDevice.DeletePortMapAsync(mapping).ConfigureAwait(false);
            }
            catch (MappingException)
            {
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "[NAT] Searching for UPnP devices. This will take 5 seconds.")]
    private partial void PrintNatSearchForUpnpDevices();

    [LoggerMessage(Level = LogLevel.Information, Message = "[NAT] Found {amount} UPnP devices.")]
    private partial void PrintNatFoundUpnpDevices(int amount);

    [LoggerMessage(Level = LogLevel.Information, Message = "[NAT] Created new UPnP port mapping for interface {ipAddress}.")]
    private partial void PrintNatCreatedUpnpDeviceMapping(IPAddress ipAddress);
}