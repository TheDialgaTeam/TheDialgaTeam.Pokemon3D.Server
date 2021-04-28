using System.Collections.Generic;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Options;
using TheDialgaTeam.Pokemon3D.Server.Packages;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Players
{
    internal class PlayerCollection
    {
        private readonly Logger _logger;
        private readonly IOptionsMonitor<ServerOptions> _optionsMonitor;

        public SortedDictionary<int, Player> Players { get; } = new();

        public PlayerCollection(Logger logger, IOptionsMonitor<ServerOptions> optionsMonitor)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
        }

        public Player? Add(PlayerNetwork playerNetwork)
        {
            var id = GetNextRunningNumber();
            if (id == -1) return null;

            var player = new Player(id, playerNetwork);
            Players.Add(id, player);
            return player;
        }

        public void SendToPlayer(int player, Package package)
        {
            Players[player].PlayerNetwork.EnqueuePackage(package);
        }

        public void SendToAllPlayers(Package package)
        {
            foreach (var player in Players)
            {
                player.Value.PlayerNetwork.EnqueuePackage(package);
            }
        }

        private int GetNextRunningNumber()
        {
            var serverOptions = _optionsMonitor.CurrentValue;

            if (Players.Count + 1 >= serverOptions.MaxPlayers) return -1;

            for (var i = 0; i < serverOptions.MaxPlayers; i++)
            {
                if (!Players.ContainsKey(i)) return i;
            }

            return -1;
        }
    }
}