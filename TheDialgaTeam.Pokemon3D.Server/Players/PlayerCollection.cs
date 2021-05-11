using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Network;
using TheDialgaTeam.Pokemon3D.Server.Options.Server;
using TheDialgaTeam.Pokemon3D.Server.Packages;

namespace TheDialgaTeam.Pokemon3D.Server.Players
{
    internal class PlayerCollection
    {
        private readonly IOptionsMonitor<ServerOptions> _optionsMonitor;

        public ObservableCollection<Player> Players { get; } = new();

        public PlayerCollection(IOptionsMonitor<ServerOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public Player? Add(TcpClientNetwork tcpClientNetwork)
        {
            var id = GetNextRunningNumber();
            if (id == -1) return null;

            var player = new Player(id, tcpClientNetwork);
            Players.Insert(id - 1, player);
            return player;
        }

        public void Remove(int id)
        {
            Players.Remove(Players[id - 1]);
        }

        public void SendToPlayer(int player, Package package)
        {
            Players[player].TcpClientNetwork.EnqueuePackage(package);
        }

        public void SendToAllPlayers(Package package)
        {
            foreach (var player in Players)
            {
                player.TcpClientNetwork.EnqueuePackage(package);
            }
        }

        private int GetNextRunningNumber()
        {
            var serverOptions = _optionsMonitor.CurrentValue;

            if (Players.Count + 1 > serverOptions.MaxPlayers) return -1;

            for (var i = 0; i < Players.Count; i++)
            {
                var id = i + 1;
                if (Players[i].Id != id) return id;
            }

            return Players.Count + 1;
        }
    }
}