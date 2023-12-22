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
    public static async Task<INatDevice?> DiscoverNatDeviceAsync(CancellationToken cancellationToken = default)
    {
        INatDevice? devices = null;
        var natDeviceFoundTcs = new TaskCompletionSource();
        
        NatUtility.DeviceFound += NatUtilityOnDeviceFound;
        NatUtility.StartDiscovery(NatProtocol.Upnp);

        await Task.WhenAny(natDeviceFoundTcs.Task, Task.Delay(-1, cancellationToken)).ConfigureAwait(false);
        
        NatUtility.DeviceFound -= NatUtilityOnDeviceFound;
        NatUtility.StopDiscovery();

        return devices;

        void NatUtilityOnDeviceFound(object? sender, DeviceEventArgs e)
        {
            devices = e.Device;
            natDeviceFoundTcs.SetResult();
        }
    }
    
    public static async Task<Mapping?> CreatePortMappingAsync(INatDevice natDevice, IPEndPoint targetIpEndPoint)
    {
        try
        {
            var natDeviceMapping = await natDevice.GetSpecificMappingAsync(Protocol.Tcp, targetIpEndPoint.Port).ConfigureAwait(false);

            if (natDeviceMapping.IsExpired())
            {
                await natDevice.DeletePortMapAsync(natDeviceMapping).ConfigureAwait(false);
            }
            else
            {
                return natDeviceMapping;
            }
        }
        catch (MappingException)
        {
        }

        try
        {
            return await natDevice.CreatePortMapAsync(new Mapping(Protocol.Tcp, targetIpEndPoint.Port, targetIpEndPoint.Port)).ConfigureAwait(false);
        }
        catch (MappingException)
        {
        }

        return null;
    }

    public static async Task DestroyPortMappingAsync(INatDevice natDevice, IPEndPoint targetIpEndPoint)
    {
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