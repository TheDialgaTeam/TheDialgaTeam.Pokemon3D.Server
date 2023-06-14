using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mono.Nat;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Options;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Nat;

public sealed partial class NatDeviceListener : IDisposable
{
    private readonly ILogger<NatDeviceListener> _logger;

    private NetworkOptions _networkOptions;
    private readonly IDisposable? _networkOptionsDisposable;
    
    private int _isActive;
    private CancellationTokenSource? _cancellationTokenSource;

    private Task? _natDevicePortMappingTask;

    public NatDeviceListener(ILogger<NatDeviceListener> logger, IOptionsMonitor<NetworkOptions> optionsMonitor)
    {
        _logger = logger;
        _networkOptions = optionsMonitor.CurrentValue;
        _networkOptionsDisposable = optionsMonitor.OnChange(options => _networkOptions = options);
    }

    public async Task StartAsync()
    {
        if (Interlocked.CompareExchange(ref _isActive, 1, 0) == 1) return;

        _cancellationTokenSource = new CancellationTokenSource();
        
        var natDevices = await DiscoverNatDevicesAsync().ConfigureAwait(false);
        await CreatePortMappingAsync(natDevices).ConfigureAwait(false);

        _natDevicePortMappingTask = Task.Factory.StartNew(() => RunNatDevicePortMappingTask(natDevices), TaskCreationOptions.LongRunning).Unwrap();
    }

    public async Task StopAsync()
    {
        if (Interlocked.CompareExchange(ref _isActive, 0, 1) == 0) return;
        
        _cancellationTokenSource?.Cancel();

        if (_natDevicePortMappingTask != null)
        {
            await _natDevicePortMappingTask.ConfigureAwait(false);
        }
        
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
    
    private async Task<INatDevice[]> DiscoverNatDevicesAsync()
    {
        PrintSearchForUpnpDevices();
            
        var devices = new List<INatDevice>(1);
        
        void NatUtilityOnDeviceFound(object? sender, DeviceEventArgs e)
        {
            devices.Add(e.Device);
        }
        
        try
        {
            NatUtility.DeviceFound += NatUtilityOnDeviceFound;
            NatUtility.StartDiscovery(NatProtocol.Upnp, NatProtocol.Pmp);
                
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await Task.Delay(-1, cancellationTokenSource.Token).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            NatUtility.DeviceFound -= NatUtilityOnDeviceFound;
            NatUtility.StopDiscovery();
        }

        PrintFoundUpnpDevices(devices.Count);

        return devices.ToArray();
    }
    
    private async Task CreatePortMappingAsync(IEnumerable<INatDevice> natDevices)
    {
        var portToForward = _networkOptions.BindIpEndPoint.Port;
        
        foreach (var natDevice in natDevices)
        {
            var createMapping = true;
                
            try
            {
                var mapping = await natDevice.GetSpecificMappingAsync(Protocol.Tcp, portToForward).ConfigureAwait(false);
                
                if (mapping.IsExpired())
                {
                    PrintExpiredUpnpDeviceMapping(natDevice.DeviceEndpoint);
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
                PrintCreatedUpnpDeviceMapping(natDevice.DeviceEndpoint);
            }
        }
    }

    private async Task RunNatDevicePortMappingTask(INatDevice[] natDevices)
    {
        Debug.Assert(_cancellationTokenSource != null, nameof(_cancellationTokenSource) + " != null");

        var cancellationToken = _cancellationTokenSource.Token;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await CreatePortMappingAsync(natDevices).ConfigureAwait(false);
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
    
    [LoggerMessage(0, LogLevel.Information, "[NAT] Searching for NAT devices. This will take 5 seconds.")]
    private partial void PrintSearchForUpnpDevices();

    [LoggerMessage(0, LogLevel.Information, "[NAT] Found {amount} UPnP devices.")]
    private partial void PrintFoundUpnpDevices(int amount);

    [LoggerMessage(0, LogLevel.Information, "[NAT {ipEndPoint}] Found expired UPnP port mapping.")]
    private partial void PrintExpiredUpnpDeviceMapping(IPEndPoint ipEndPoint);

    [LoggerMessage(0, LogLevel.Information, "[NAT {ipEndPoint}] Created new UPnP port mapping.")]
    private partial void PrintCreatedUpnpDeviceMapping(IPEndPoint ipEndPoint);

    public void Dispose()
    {
        _networkOptionsDisposable?.Dispose();
    }
}