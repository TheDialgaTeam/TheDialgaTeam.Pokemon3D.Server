using System;
using System.Threading.Tasks;
using TheDialgaTeam.Pokemon3D.Server.Network;
using TheDialgaTeam.Pokemon3D.Server.Network.Game;
using TheDialgaTeam.Pokemon3D.Server.Serilog;
using TheDialgaTeam.Pokemon3D.Server.Worlds;

namespace TheDialgaTeam.Pokemon3D.Server
{
    internal class GameServer : IDisposable
    {
        private readonly Logger _logger;
        private readonly NatDevices _natDevices;
        private readonly World _world;
        private readonly TcpClientListener _tcpClientListener;

        private bool _isStarted;

        public GameServer(Logger logger, NatDevices natDevices, World world, TcpClientListener tcpClientListener)
        {
            _logger = logger;
            _natDevices = natDevices;
            _world = world;
            _tcpClientListener = tcpClientListener;
        }

        public async Task StartServerAsync()
        {
            if (_isStarted) return;
            _isStarted = true;

            await _natDevices.OpenPortAsync();
            _world.StartWorld();
            _tcpClientListener.StartListening();
        }

        public void StopServer()
        {
            if (!_isStarted) return;
            _isStarted = false;

            _world.StopWorld();
            _tcpClientListener.StopListening();
        }

        public void Dispose()
        {
            StopServer();
        }
    }
}