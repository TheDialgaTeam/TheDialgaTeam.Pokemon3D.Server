using Mediator;
using Microsoft.Extensions.Hosting;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Commands;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

namespace TheDialgaTeam.Pokemon3D.Server.Cli;

internal sealed class ConsoleService : BackgroundService
{
    private readonly IMediator _mediator;

    public ConsoleService(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.Title = $"{ApplicationUtility.Name} v{ApplicationUtility.Version} ({ApplicationUtility.FrameworkVersion})";

        await _mediator.Send(new StartServer(), stoppingToken).ConfigureAwait(false);

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

        await _mediator.Send(new StopServer(), stoppingToken).ConfigureAwait(false);
    }
}