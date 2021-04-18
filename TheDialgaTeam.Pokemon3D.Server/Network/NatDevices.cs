using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Open.Nat;
using TheDialgaTeam.Pokemon3D.Server.Options;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Network
{
    internal class NatDevices : IDisposable
    {
        private readonly Logger _logger;
        private readonly IOptionsMonitor<NetworkOptions> _networkOptionsMonitor;

        private readonly NatDiscoverer _natDiscoverer = new();

        public NatDevices(Logger logger, IOptionsMonitor<NetworkOptions> networkOptionsMonitor)
        {
            _logger = logger;
            _networkOptionsMonitor = networkOptionsMonitor;
        }

        private static async Task<bool> IsPortCreatedAsync(NatDevice natDevice, int targetPort)
        {
            try
            {
                var mappings = await natDevice.GetAllMappingsAsync();

                foreach (var mapping in mappings)
                {
                    if (mapping.PublicPort == targetPort)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (MappingException)
            {
                return false;
            }
        }

        public async Task OpenPortAsync(CancellationToken cancellationToken = default)
        {
            var networkOptions = _networkOptionsMonitor.CurrentValue;

            PortMapper portMapper = 0;

            if (networkOptions.UseUniversalPlugAndPlay) portMapper |= PortMapper.Upnp;
            if (networkOptions.UsePortMappingProtocol) portMapper |= PortMapper.Pmp;

            if (portMapper == 0) return;

            var portsToOpen = new List<int>();

            if (IPAddress.TryParse(networkOptions.Game.BindIpAddress, out var gameIpAddress))
            {
                if (!gameIpAddress.Equals(IPAddress.Loopback)) portsToOpen.Add(networkOptions.Game.Port);
            }

            if (IPAddress.TryParse(networkOptions.Rpc.BindIpAddress, out var rpcIpAddress))
            {
                if (!rpcIpAddress.Equals(IPAddress.Loopback)) portsToOpen.Add(networkOptions.Rpc.Port);
            }

            if (portsToOpen.Count == 0) return;

            using var cancellationTokenSource = new CancellationTokenSource(5000);
            using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);

            _logger.LogInformation("Discovering NAT devices, please wait...", true);

            var devices = await _natDiscoverer.DiscoverDevicesAsync(portMapper, linkedCancellationTokenSource);

            foreach (var natDevice in devices)
            {
                foreach (var portToOpen in portsToOpen)
                {
                    if (await IsPortCreatedAsync(natDevice, portToOpen))
                    {
                        _logger.LogInformation("\u001b[32;1m[{Host:l}] Port {Port} has already been opened for public communication\u001b[0m", true, natDevice.LocalAddress, portToOpen);
                        continue;
                    }

                    try
                    {
                        await natDevice.CreatePortMapAsync(new Mapping(Protocol.Tcp, portToOpen, portToOpen, "Pokemon 3D Server"));
                        _logger.LogInformation("\u001b[32;1m[{Host:l}] Port {Port} has successfully opened for public communication\u001b[0m", true, natDevice.LocalAddress, portToOpen);
                    }
                    catch (MappingException mappingException)
                    {
                        _logger.LogError(mappingException, "[{Host:l}] Unable to create port mapping for port {portToOpen}", true, natDevice.LocalAddress, portToOpen);
                    }
                }
            }
        }

        public void Dispose()
        {
            NatDiscoverer.ReleaseAll();
        }
    }
}