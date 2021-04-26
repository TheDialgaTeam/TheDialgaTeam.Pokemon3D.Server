using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Options;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Players
{
    internal class PlayerCollection : IDisposable
    {
        private readonly Logger _logger;
        private readonly IOptionsMonitor<ServerOptions> _optionsMonitor;

        private readonly IDisposable _optionsListenerDisposable;

        private int _maxPlayers;

        public SortedDictionary<int, PlayerNetwork> Players { get; } = new();

        public PlayerCollection(Logger logger, IOptionsMonitor<ServerOptions> optionsMonitor)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;

            _optionsListenerDisposable = optionsMonitor.OnChange(options =>
            {
                _maxPlayers = options.MaxPlayers;
            });

            _maxPlayers = optionsMonitor.CurrentValue.MaxPlayers;
        }

        public Player? Add(PlayerNetwork playerNetwork)
        {
            var id = GetNextRunningNumber();
            if (id == -1) return null;

            Players.Add(id, playerNetwork);
            return new Player(id);
        }

        private int GetNextRunningNumber()
        {
            for (var i = 0; i < _maxPlayers; i++)
            {
                if (!Players.ContainsKey(i)) return i;
            }

            return -1;
        }

        public void Dispose()
        {
            _optionsListenerDisposable.Dispose();
        }
    }
}