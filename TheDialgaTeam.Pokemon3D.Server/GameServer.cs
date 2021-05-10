using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheDialgaTeam.Pokemon3D.Server.Database;
using TheDialgaTeam.Pokemon3D.Server.Network;
using TheDialgaTeam.Pokemon3D.Server.Worlds;

namespace TheDialgaTeam.Pokemon3D.Server
{
    internal class GameServer : IDisposable
    {
        private readonly NatDevices _natDevices;
        private readonly World _world;
        private readonly TcpClientListener _tcpClientListener;
        private readonly IDbContextFactory<SqliteDatabaseContext> _dbContextFactory;

        private bool _isStarted;

        public GameServer(NatDevices natDevices, World world, TcpClientListener tcpClientListener, IDbContextFactory<SqliteDatabaseContext> dbContextFactory)
        {
            _natDevices = natDevices;
            _world = world;
            _tcpClientListener = tcpClientListener;
            _dbContextFactory = dbContextFactory;
        }

        public async Task StartServerAsync()
        {
            if (_isStarted) return;
            _isStarted = true;

            await using (var context = _dbContextFactory.CreateDbContext())
            {
                await context.Database.MigrateAsync();
            }

            _natDevices.StartDiscovering();
            _world.StartWorld();
            _tcpClientListener.StartListening();
        }

        public void StopServer()
        {
            if (!_isStarted) return;
            _isStarted = false;

            _natDevices.StopDiscovering();
            _world.StopWorld();
            _tcpClientListener.StopListening();
        }

        public void Dispose()
        {
            StopServer();
        }
    }
}