using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TheDialgaTeam.Pokemon3D.Server.Core.Extensions;
using TheDialgaTeam.Serilog.Extensions;
using TheDialgaTeam.Serilog.Sinks.Action;
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
            .ConfigureSerilog((context, provider, configuration) =>
            {
                if (context.Configuration.GetValue("TheDialgaTeam.Pokemon3D.Server.Cli:GuiMode", false))
                {
                    configuration.MinimumLevel.Verbose().WriteTo.ActionSink(provider);
                }
                else
                {
                    configuration.MinimumLevel.Verbose().WriteTo.AnsiConsoleSink(provider);
                }
            })
            .ConfigureServices(static (context, collection) =>
            {
                collection.AddMediator();
                
                if (context.Configuration.GetValue("TheDialgaTeam.Pokemon3D.Server.Cli:GuiMode", false))
                {
                    collection.AddHostedService<ConsoleGuiService>();
                    collection.AddHostedService<ServerHostedService>();
                }
                else
                {
                    collection.AddHostedService<ConsoleBackgroundService>();
                    collection.AddHostedService<ServerHostedService>();
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