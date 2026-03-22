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

using Mono.Nat;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Network.Upnp;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Upnp;

internal class NatDeviceUtility : INatDeviceUtility
{
    private static async Task<INatDevice?> DiscoverNatDeviceAsync(CancellationToken cancellationToken = default)
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
    
    public async Task<int> CreatePortMappingAsync(int port, CancellationToken cancellationToken = default)
    {
        var natDevice = await DiscoverNatDeviceAsync(cancellationToken).ConfigureAwait(false);

        if (natDevice == null)
        {
            throw new MappingException("Could not find NAT device");
        }
        
        try
        {
            var natDeviceMapping = await natDevice.GetSpecificMappingAsync(Protocol.Tcp, port).ConfigureAwait(false);

            if (natDeviceMapping.IsExpired())
            {
                await natDevice.DeletePortMapAsync(natDeviceMapping).ConfigureAwait(false);
            }
            else
            {
                return natDeviceMapping.PrivatePort;
            }
        }
        catch (MappingException)
        {
        }

        return (await natDevice.CreatePortMapAsync(new Mapping(Protocol.Tcp, port, port, 0, "Pokemon 3D Listener Port")).ConfigureAwait(false)).PrivatePort;
    }

    public async Task DestroyPortMappingAsync(int port, CancellationToken cancellationToken = default)
    {
        var natDevice = await DiscoverNatDeviceAsync(cancellationToken).ConfigureAwait(false);

        if (natDevice == null)
        {
            throw new MappingException("Could not find NAT device");
        }
        
        try
        {
            var natDeviceMapping = await natDevice.GetSpecificMappingAsync(Protocol.Tcp, port).ConfigureAwait(false);
            await natDevice.DeletePortMapAsync(natDeviceMapping).ConfigureAwait(false);
        }
        catch (MappingException)
        {
        }
    }
}