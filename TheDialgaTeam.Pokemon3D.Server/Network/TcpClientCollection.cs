using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Database;
using TheDialgaTeam.Pokemon3D.Server.Options.Server;
using TheDialgaTeam.Pokemon3D.Server.Players;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Network
{
    internal class TcpClientCollection : IDisposable
    {
        private readonly Logger _logger;
        private readonly IOptionsMonitor<ServerOptions> _optionsMonitor;
        private readonly PlayerCollection _playerCollection;
        private readonly IDbContextFactory<SqliteDatabaseContext> _dbContextFactory;

        private readonly List<TcpClientNetwork> _connectedNetworks;

        public TcpClientCollection(Logger logger, IOptionsMonitor<ServerOptions> optionsMonitor, PlayerCollection playerCollection, IDbContextFactory<SqliteDatabaseContext> dbContextFactory)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _playerCollection = playerCollection;
            _dbContextFactory = dbContextFactory;

            _connectedNetworks = new List<TcpClientNetwork>(_optionsMonitor.CurrentValue.MaxPlayers + 1);
        }

        public void Add(TcpClient tcpClient)
        {
            var tcpClientNetwork = new TcpClientNetwork(_logger, _optionsMonitor, this, _playerCollection, _dbContextFactory, tcpClient);
            tcpClientNetwork.StartNetwork();

            _connectedNetworks.Add(tcpClientNetwork);
        }

        public void Remove(TcpClientNetwork tcpClientNetwork)
        {
            _connectedNetworks.Remove(tcpClientNetwork);
        }

        public void Dispose()
        {
            foreach (var connectedNetwork in _connectedNetworks)
            {
                connectedNetwork.Dispose();
            }
        }
    }
}