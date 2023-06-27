using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Hosting;
using TheDialgaTeam.Microsoft.Extensions.Logging;
using TheDialgaTeam.Microsoft.Extensions.Logging.Action;
using TheDialgaTeam.Pokemon3D.Server.Core.Extensions;

namespace TheDialgaTeam.Pokemon3D.Server.Gui;

public static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    
    [STAThread]
    public static int Main(string[] args)
    {
        return BuildAvaloniaApp()
            .AfterSetup(builder =>
            {
                var host = CreateHost(args);
                ServiceProvider = host.Services;
                host.Start();
                
                if (builder.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
                {
                    lifetime.ShutdownRequested += (_, _) =>
                    {
                        host.StopAsync().GetAwaiter().GetResult();
                    };
                }
            })
            .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUI();
    }

    private static IHost CreateHost(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices(collection =>
            {
                collection.AddPokemonServer();
            })
            .ConfigureLogging(builder =>
            {
                builder.AddActionLogger(options =>
                {
                    options.SetDefaultTemplate(static formattingBuilder => formattingBuilder.SetGlobal(static messageFormattingBuilder => messageFormattingBuilder.SetPrefix(static (in LoggingTemplateEntry _) => $"{AnsiEscapeCodeConstants.DarkGrayForegroundColor}{DateTime.Now:yyyy-MM-dd HH:mm:ss}{AnsiEscapeCodeConstants.Reset} ")));
                });
            })
            .Build();
    }
}