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
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Upnp.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Upnp.Factory;

internal class MonoNatDeviceServiceFactory : INatDeviceServiceFactory
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

    public async Task<INatDeviceService> GetAsync(CancellationToken cancellationToken = default)
    {
        var natDevice = await DiscoverNatDeviceAsync(cancellationToken).ConfigureAwait(false);
        return natDevice is null ? throw new Exception("No NAT device found.") : new MonoNatDeviceService(natDevice);
    }
}