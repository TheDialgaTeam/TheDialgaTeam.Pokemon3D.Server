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
using Mono.Nat;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Upnp.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Upnp;

internal class MonoNatDeviceService(INatDevice natDevice) : INatDeviceService
{
    public IPEndPoint DeviceEndpoint => natDevice.DeviceEndpoint;
    
    public async Task<int> CreatePortMappingAsync(int port, CancellationToken cancellationToken = default)
    {
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
        
        return (await natDevice.CreatePortMapAsync(new Mapping(Protocol.Tcp, port, port)).ConfigureAwait(false)).PrivatePort;
    }

    public async Task DestroyPortMappingAsync(int port, CancellationToken cancellationToken = default)
    {
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