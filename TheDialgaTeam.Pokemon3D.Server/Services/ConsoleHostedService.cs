using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TheDialgaTeam.Pokemon3D.Server.Frontend.Console.Views;

namespace TheDialgaTeam.Pokemon3D.Server.Services
{
    internal class ConsoleHostedService : IHostedService
    {
        private readonly ConsoleWindow _consoleWindow;

        public ConsoleHostedService(ConsoleWindow consoleWindow)
        {
            _consoleWindow = consoleWindow;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _consoleWindow.Initialize();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}