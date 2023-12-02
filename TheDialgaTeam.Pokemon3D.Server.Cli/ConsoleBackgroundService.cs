using Mediator;
using Microsoft.Extensions.Hosting;
using TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

namespace TheDialgaTeam.Pokemon3D.Server.Cli;

internal sealed class ConsoleBackgroundService : BackgroundService
{
    private readonly IMediator _mediator;

    public ConsoleBackgroundService(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.Title = $"{ApplicationUtility.Name} v{ApplicationUtility.Version} ({ApplicationUtility.FrameworkVersion})";
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var command = Console.ReadLine();
            if (command == null) break;
            
            // TODO: Handle command
        }
        
        return Task.CompletedTask;
    }
}