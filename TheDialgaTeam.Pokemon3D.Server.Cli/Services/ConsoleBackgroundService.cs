using Microsoft.Extensions.Hosting;
using TheDialgaTeam.Pokemon3D.Server.Core.Utilities;
using ApplicationUtility = TheDialgaTeam.Pokemon3D.Server.Cli.Utilities.ApplicationUtility;

namespace TheDialgaTeam.Pokemon3D.Server.Cli.Services;

internal sealed class ConsoleBackgroundService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(() =>
        {
            Console.Title = $"{ApplicationUtility.Name} v{ApplicationUtility.Version} ({ApplicationUtility.FrameworkVersion})";
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var command = Console.ReadLine();
                if (command == null) break;

                // TODO: Handle command
            }
        }, stoppingToken);
    }
}