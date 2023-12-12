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
using Mono.Nat;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

public static class NatDeviceUtility
{
    public static async Task<INatDevice[]> DiscoverNatDevicesAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Array.Empty<INatDevice>();
        }
        
        var devices = new List<INatDevice>();

        NatUtility.DeviceFound += NatUtilityOnDeviceFound;
        NatUtility.StartDiscovery(NatProtocol.Upnp);
        
        await Task.WhenAny(Task.Delay(-1, cancellationToken)).ConfigureAwait(false);
        
        NatUtility.DeviceFound -= NatUtilityOnDeviceFound;
        NatUtility.StopDiscovery();

        return devices.ToArray();

        void NatUtilityOnDeviceFound(object? sender, DeviceEventArgs e)
        {
            devices.Add(e.Device);
        }
    }
    
    public static async Task<(INatDevice natDevice, Mapping mapping)[]> CreatePortMappingAsync(INatDevice[] natDevices, IPEndPoint targetIpEndPoint)
    {
        var mappingsCreated = new List<(INatDevice natDevice, Mapping mapping)>();
        
        foreach (var natDevice in natDevices)
        {
            if (!targetIpEndPoint.Address.Equals(IPAddress.Any) && 
                !targetIpEndPoint.Address.Equals(natDevice.DeviceEndpoint.Address)) continue;

            var createMapping = true;

            try
            {
                var natDeviceMapping = await natDevice.GetSpecificMappingAsync(Protocol.Tcp, targetIpEndPoint.Port).ConfigureAwait(false);

                if (natDeviceMapping.IsExpired())
                {
                    await natDevice.DeletePortMapAsync(natDeviceMapping).ConfigureAwait(false);
                }
                else
                {
                    mappingsCreated.Add((natDevice, natDeviceMapping));
                    createMapping = false;
                }
            }
            catch (MappingException)
            {
            }

            if (!createMapping) continue;

            try
            {
                var mappingCreated = await natDevice.CreatePortMapAsync(new Mapping(Protocol.Tcp, targetIpEndPoint.Port, targetIpEndPoint.Port)).ConfigureAwait(false);
                mappingsCreated.Add((natDevice, mappingCreated));
            }
            catch (MappingException)
            {
            }
        }

        return mappingsCreated.ToArray();
    }

    public static async Task DestroyPortMappingAsync(INatDevice[] natDevices, IPEndPoint targetIpEndPoint)
    {
        foreach (var natDevice in natDevices)
        {
            if (!targetIpEndPoint.Address.Equals(IPAddress.Any) && 
                !targetIpEndPoint.Address.Equals(natDevice.DeviceEndpoint.Address)) continue;

            try
            {
                var natDeviceMapping = await natDevice.GetSpecificMappingAsync(Protocol.Tcp, targetIpEndPoint.Port).ConfigureAwait(false);
                await natDevice.DeletePortMapAsync(natDeviceMapping).ConfigureAwait(false);
            }
            catch (MappingException)
            {
            }
        }
    }
}