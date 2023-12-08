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
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

internal sealed partial class NatDevicePortMapper : INatDevicePortMapper
{
    private readonly IPokemonServerOptions _options;
    private readonly ILogger _logger;
    private readonly IStringLocalizer _stringLocalizer;

    private INatDevice[] _natDevices = Array.Empty<INatDevice>();
    private IPEndPoint _targetEndPoint;
    
    public NatDevicePortMapper(IPokemonServerOptions options, ILogger<NatDevicePortMapper> logger, IStringLocalizer stringLocalizer)
    {
        _options = options;
        _logger = logger;
        _stringLocalizer = stringLocalizer;
        _targetEndPoint = IPEndPoint.Parse(options.NetworkOptions.BindingInformation);
    }

    private static async Task<INatDevice[]> DiscoverNatDevicesAsync(CancellationToken cancellationToken)
    {
        var devices = new List<INatDevice>();

        try
        {
            NatUtility.DeviceFound += NatUtilityOnDeviceFound;
            NatUtility.StartDiscovery(NatProtocol.Upnp);
            
            await Task.Delay(-1, cancellationToken).ConfigureAwait(false);
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
        PrintInformation(_stringLocalizer[s => s.ConsoleMessageFormat.NatSearchForUpnpDevices, TimeSpan.FromSeconds(_options.NetworkOptions.UpnpDiscoveryTime).TotalSeconds]);
        
        using var upnpCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(_options.NetworkOptions.UpnpDiscoveryTime));
        using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, upnpCancellationTokenSource.Token);

        _natDevices = await DiscoverNatDevicesAsync(linkedCancellationTokenSource.Token).ConfigureAwait(false);
        
        PrintInformation(_stringLocalizer[s => s.ConsoleMessageFormat.NatFoundUpnpDevices, _natDevices.Length]);

        _targetEndPoint = IPEndPoint.Parse(_options.NetworkOptions.BindingInformation);

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
            
            PrintInformation(_stringLocalizer[s => s.ConsoleMessageFormat.NatNatCreatedUpnpDeviceMapping, natDevice.DeviceEndpoint.Address]);
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

    [LoggerMessage(LogLevel.Information, "[NAT] {Message}")]
    private partial void PrintInformation(string message);
}