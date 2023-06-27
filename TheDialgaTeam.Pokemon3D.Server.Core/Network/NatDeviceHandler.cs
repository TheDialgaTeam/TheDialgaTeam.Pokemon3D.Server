using System.Net;
using Microsoft.Extensions.Logging;
using Mono.Nat;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public sealed partial class PokemonServer
{
    private INatDevice[] _natDevices = Array.Empty<INatDevice>();
    
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
    
    private async Task CreatePortMappingAsync(CancellationToken cancellationToken = default)
    {
        var portToForward = _options.NetworkOptions.BindIpEndPoint.Port;
        
        PrintNatSearchForUpnpDevices();
        _natDevices = await DiscoverNatDevicesAsync(cancellationToken).ConfigureAwait(false);
        PrintNatFoundUpnpDevices(_natDevices.Length);

        foreach (var natDevice in _natDevices)
        {
            var createMapping = true;

            try
            {
                var mapping = await natDevice.GetSpecificMappingAsync(Protocol.Tcp, portToForward).ConfigureAwait(false);

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

            if (createMapping)
            {
                await natDevice.CreatePortMapAsync(new Mapping(Protocol.Tcp, portToForward, portToForward)).ConfigureAwait(false);
                PrintNatCreatedUpnpDeviceMapping(natDevice.DeviceEndpoint.Address);
            }
        }
    }

    private async Task DestroyPortMappingAsync()
    {
        var portToForward = _options.NetworkOptions.BindIpEndPoint.Port;
        
        foreach (var natDevice in _natDevices)
        {
            try
            {
                var mapping = await natDevice.GetSpecificMappingAsync(Protocol.Tcp, portToForward).ConfigureAwait(false);
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