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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

internal sealed partial class NatDeviceUtility : INatDeviceUtility
{
    private readonly ILogger<NatDeviceUtility> _logger;

    private INatDevice[] _natDevices = Array.Empty<INatDevice>();

    public NatDeviceUtility(ILogger<NatDeviceUtility> logger)
    {
        _logger = logger;
    }

    private static async Task<INatDevice[]> DiscoverNatDevicesAsync(CancellationToken cancellationToken)
    {
        var devices = new List<INatDevice>();

        void NatUtilityOnDeviceFound(object? sender, DeviceEventArgs e)
        {
            devices.Add(e.Device);
        }

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
    }

    public async Task CreatePortMappingAsync(IPEndPoint endPoint, CancellationToken cancellationToken = default)
    {
        PrintNatSearchForUpnpDevices();
        _natDevices = await DiscoverNatDevicesAsync(cancellationToken).ConfigureAwait(false);
        PrintNatFoundUpnpDevices(_natDevices.Length);

        foreach (var natDevice in _natDevices)
        {
            if (!endPoint.Address.Equals(IPAddress.Any) && !endPoint.Address.Equals(natDevice.DeviceEndpoint.Address)) continue;

            var createMapping = true;

            try
            {
                var mapping = await natDevice.GetSpecificMappingAsync(Protocol.Tcp, endPoint.Port).ConfigureAwait(false);

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

            await natDevice.CreatePortMapAsync(new Mapping(Protocol.Tcp, endPoint.Port, endPoint.Port)).ConfigureAwait(false);
            PrintNatCreatedUpnpDeviceMapping(natDevice.DeviceEndpoint.Address);
        }
    }

    public async Task DestroyPortMappingAsync(IPEndPoint endPoint)
    {
        foreach (var natDevice in _natDevices)
        {
            if (!endPoint.Address.Equals(IPAddress.Any) && !endPoint.Address.Equals(natDevice.DeviceEndpoint.Address)) continue;

            try
            {
                var mapping = await natDevice.GetSpecificMappingAsync(Protocol.Tcp, endPoint.Port).ConfigureAwait(false);
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