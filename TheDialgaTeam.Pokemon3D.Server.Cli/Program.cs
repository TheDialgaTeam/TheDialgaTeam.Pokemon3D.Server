using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using TheDialgaTeam.Pokemon3D.Server.Core.Extensions;
using TheDialgaTeam.Serilog.Extensions;
using TheDialgaTeam.Serilog.Sinks.AnsiConsole;

namespace TheDialgaTeam.Pokemon3D.Server.Cli;

internal static class Program
{
    [UnconditionalSuppressMessage("Trimming", "IL2026")]
    public static Task Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainOnUnhandledException;

        return Host.CreateDefaultBuilder(args)
            .ConfigurePokemonServer()
            .ConfigureSerilog((_, provider, configuration) => configuration.WriteTo.AnsiConsoleSink(provider))
            .ConfigureServices(static (context, collection) =>
            {
                collection.AddMediator();
                collection.AddHostedService<ServerHostedService>();
                
                if (context.Configuration.GetValue("TheDialgaTeam.Pokemon3D.Server.Cli:GuiMode", false))
                {
                    collection.AddHostedService<ConsoleGuiService>();
                }
                else
                {
                    collection.AddHostedService<ConsoleBackgroundService>();
                }
            })
            .RunConsoleAsync(static options => options.SuppressStatusMessages = true);
    }
    
    private static void OnCurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs eventArgs)
    {
        if (!eventArgs.IsTerminating) return;

        var crashFileLocation = Path.Combine(AppContext.BaseDirectory, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_crash.log");
        File.WriteAllText(crashFileLocation, eventArgs.ExceptionObject.ToString());
    }
}