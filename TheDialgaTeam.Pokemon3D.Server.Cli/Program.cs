﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheDialgaTeam.Pokemon3D.Server.Cli.Services;
using TheDialgaTeam.Pokemon3D.Server.Core.Extensions;
using TheDialgaTeam.Serilog.Extensions;
using TheDialgaTeam.Serilog.Formatting;
using TheDialgaTeam.Serilog.Sinks.Action;
using TheDialgaTeam.Serilog.Sinks.AnsiConsole;

namespace TheDialgaTeam.Pokemon3D.Server.Cli;

internal static class Program
{
    public static Task Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainOnUnhandledException;
        
        return Host.CreateDefaultBuilder(args)
            .ConfigurePokemonServer()
            .ConfigureServices(static (context, collection) =>
            {
                collection.AddMediator();
                
                if (bool.Parse(context.Configuration["TheDialgaTeam.Pokemon3D.Server.Cli:GuiMode"] ?? "false"))
                {
                    collection.AddSingleton<ActionSinkOptions>();
                    collection.AddHostedService<ConsoleGuiService>();
                    collection.AddHostedService<ServerHostedService>();
                }
                else
                {
                    collection.AddHostedService<ConsoleBackgroundService>();
                    collection.AddHostedService<ServerHostedService>();
                }
            })
            .ConfigureSerilog(static (context, provider, configuration) =>
            {
                if (bool.Parse(context.Configuration["TheDialgaTeam.Pokemon3D.Server.Cli:GuiMode"] ?? "false"))
                {
                    configuration.WriteTo.ActionSink(new AnsiMessageTemplateTextFormatter(new LogLevelMessageTemplateOptions()), provider.GetRequiredService<ActionSinkOptions>());
                }
                else
                {
                    configuration.WriteTo.AnsiConsoleSink();
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