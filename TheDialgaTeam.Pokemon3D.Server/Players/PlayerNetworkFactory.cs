using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Options;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Players
{
    internal class PlayerNetworkFactory
    {
        private readonly Logger _logger;
        private readonly IOptionsMonitor<ServerOptions> _optionsMonitor;
        private readonly PlayerCollection _playerCollection;

        public PlayerNetworkFactory(Logger logger, IOptionsMonitor<ServerOptions> optionsMonitor, PlayerCollection playerCollection)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _playerCollection = playerCollection;
        }

        public PlayerNetwork CreatePlayerNetwork()
        {
            return new(_logger, _optionsMonitor, _playerCollection);
        }
    }
}