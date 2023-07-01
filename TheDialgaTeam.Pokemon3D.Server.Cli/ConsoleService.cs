using Microsoft.Extensions.Hosting;
using TheDialgaTeam.Pokemon3D.Server.Core.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

namespace TheDialgaTeam.Pokemon3D.Server.Cli;

internal sealed class ConsoleService : BackgroundService
{
    private readonly IPokemonServer _pokemonServer;

    public ConsoleService(IPokemonServer pokemonServer)
    {
        _pokemonServer = pokemonServer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.Title = $"{ApplicationUtility.Name} v{ApplicationUtility.Version} ({ApplicationUtility.FrameworkVersion})";
        
        await _pokemonServer.StartAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var consoleTask = Task.Factory.StartNew(Console.ReadLine, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            var cancelTask = Task.Delay(-1, stoppingToken);
            var resultTask = await Task.WhenAny(consoleTask, cancelTask);
            
            if (resultTask == cancelTask) break;
            
            var command = await consoleTask;
            if (command == null) break;
            
            // TODO: Handle command
        }

        await _pokemonServer.StopAsync(stoppingToken);
    }
}