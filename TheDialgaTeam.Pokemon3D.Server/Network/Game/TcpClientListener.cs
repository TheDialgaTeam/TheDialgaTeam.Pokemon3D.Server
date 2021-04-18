using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Options;

namespace TheDialgaTeam.Pokemon3D.Server.Network.Game
{
    internal class TcpClientListener
    {
        private readonly ILogger<TcpClientListener> _logger;
        private readonly TcpListener _tcpListener;

        public TcpClientListener(ILogger<TcpClientListener> logger, IOptionsMonitor<GameNetworkOptions> optionsMonitor)
        {
            _logger = logger;

            var gameNetworkOptions = optionsMonitor.CurrentValue;

            _tcpListener = new TcpListener(IPAddress.Parse(gameNetworkOptions.BindIpAddress), gameNetworkOptions.Port);
            _tcpListener.AllowNatTraversal(true);
        }

        public void StartListening()
        {
            _tcpListener.Start();
        }

        public void StopListening()
        {
            _tcpListener.Stop();
        }
    }
}