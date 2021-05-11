using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Options;
using Mono.Nat;
using TheDialgaTeam.Core.Logger.Formatter;
using TheDialgaTeam.Pokemon3D.Server.Options.Server;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Network
{
    internal class NatDevices : IDisposable
    {
        private readonly Logger _logger;
        private readonly IOptionsMonitor<NetworkOptions> _optionsMonitor;

        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private readonly Dictionary<int, string> _portsToOpen = new();

        private readonly Dictionary<INatDevice, List<Mapping>> _mappingsCreated = new();

        public NatDevices(Logger logger, IOptionsMonitor<NetworkOptions> optionsMonitor)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                foreach (var (device, mappings) in _mappingsCreated)
                {
                    foreach (var mapping in mappings)
                    {
                        try
                        {
                            device.DeletePortMapAsync(mapping).GetAwaiter().GetResult();
                        }
                        catch (MappingException)
                        {
                            // ignored
                        }
                    }
                }
            };
        }

        public void StartDiscovering()
        {
            var networkOptions = _optionsMonitor.CurrentValue;

            if (!networkOptions.UseUniversalPlugAndPlay) return;

            _portsToOpen.Clear();

            if (IPAddress.TryParse(networkOptions.Game.BindIpAddress, out var gameIpAddress))
            {
                if (!gameIpAddress.Equals(IPAddress.Loopback)) _portsToOpen.Add(networkOptions.Game.Port, "Pokemon 3D Server Game Network");
            }

            if (IPAddress.TryParse(networkOptions.Rpc.BindIpAddress, out var rpcIpAddress))
            {
                if (!rpcIpAddress.Equals(IPAddress.Loopback)) _portsToOpen.Add(networkOptions.Rpc.Port, "Pokemon 3D Server Rpc Network");
            }

            if (_portsToOpen.Count == 0) return;

            _logger.LogInformation("[NAT] Starting NAT discovery", true);

            foreach (var (device, mappings) in _mappingsCreated)
            {
                foreach (var mapping in mappings)
                {
                    try
                    {
                        device.DeletePortMapAsync(mapping).GetAwaiter().GetResult();
                    }
                    catch (MappingException)
                    {
                        // ignored
                    }
                }
            }

            _mappingsCreated.Clear();

            NatUtility.DeviceFound += NatUtilityOnDeviceFound;
            NatUtility.StartDiscovery(NatProtocol.Upnp);

            _logger.LogInformation($"[NAT] {AnsiEscapeCodeConstants.GreenForegroundColor}NAT discovery started{AnsiEscapeCodeConstants.DefaultColor}", true);
        }

        public void StopDiscovering()
        {
            _logger.LogInformation("[NAT] Stopping NAT discovery", true);

            NatUtility.StopDiscovery();

            _logger.LogInformation($"[NAT] {AnsiEscapeCodeConstants.GreenForegroundColor}NAT discovery stopped{AnsiEscapeCodeConstants.DefaultColor}", true);
        }

        private async void NatUtilityOnDeviceFound(object? sender, DeviceEventArgs e)
        {
            try
            {
                await _semaphoreSlim.WaitAsync();

                try
                {
                    var device = e.Device;
                    var externalIpAddress = await device.GetExternalIPAsync();

                    _logger.LogInformation("[NAT] Discovered NAT device: {Host:l} - {NatProtocol:l}", true, device.DeviceEndpoint.Address.ToString(), device.NatProtocol.ToString());

                    _mappingsCreated.Add(device, new List<Mapping>());

                    foreach (var (portToOpen, description) in _portsToOpen)
                    {
                        try
                        {
                            var mapping = await device.GetSpecificMappingAsync(Protocol.Tcp, portToOpen);

                            if (mapping.PublicPort != portToOpen || mapping.PrivatePort != portToOpen || mapping.Description != description)
                            {
                                await device.DeletePortMapAsync(mapping);
                            }
                            else
                            {
                                _logger.LogWarning("[NAT] {Host:l}:{Port} has already been opened for public communication", true, externalIpAddress.ToString(), portToOpen);
                                _mappingsCreated[device].Add(mapping);
                            }
                        }
                        catch (MappingException)
                        {
                            var mappingCreated = await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, portToOpen, portToOpen, 0, description));
                            _logger.LogInformation($"[NAT] {AnsiEscapeCodeConstants.GreenForegroundColor}{{Host:l}}:{{Port}} has successfully opened for public communication{AnsiEscapeCodeConstants.DefaultColor}", true, externalIpAddress.ToString(), portToOpen);
                            _mappingsCreated[device].Add(mappingCreated);
                        }
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("[NAT] Releasing all the open ports from this session", true);

            foreach (var (device, mappings) in _mappingsCreated)
            {
                foreach (var mapping in mappings)
                {
                    try
                    {
                        device.DeletePortMapAsync(mapping).GetAwaiter().GetResult();
                    }
                    catch (MappingException)
                    {
                        // ignored
                    }
                }
            }

            _logger.LogInformation($"[NAT] {AnsiEscapeCodeConstants.GreenForegroundColor}Released all the open ports from this session{AnsiEscapeCodeConstants.DefaultColor}", true);

            _semaphoreSlim.Dispose();
        }
    }
}