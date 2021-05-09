using System.Threading;
using Microsoft.Extensions.Hosting;

namespace TheDialgaTeam.Pokemon3D.Server.Frontend.Console.ViewModels
{
    internal class ConsoleWindowViewModel
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly GameServer _gameServer;

        public CancellationToken CancellationToken => _hostApplicationLifetime.ApplicationStopping;

        public ConsoleWindowViewModel(IHostApplicationLifetime hostApplicationLifetime, GameServer gameServer)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _gameServer = gameServer;
        }

        public async void StartServer()
        {
            await _gameServer.StartServerAsync();
        }

        public void StopServer()
        {
            _gameServer.StopServer();
        }

        public void Exit()
        {
            _hostApplicationLifetime.StopApplication();
        }
    }
}