using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Avalonia.Controls;
using Microsoft.Extensions.Hosting;
using TheDialgaTeam.Microsoft.Extensions.Logging;
using TheDialgaTeam.Microsoft.Extensions.Logging.Action;
using TheDialgaTeam.Pokemon3D.Server.Core.Extensions;

namespace TheDialgaTeam.Pokemon3D.Server.Gui;

internal static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    
    [STAThread]
    public static void Main(string[] args)
    {
        var host = CreateHost(args);
        ServiceProvider = host.Services;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
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
                    options.SetDefaultTemplate(formattingBuilder => formattingBuilder.SetGlobal(messageFormattingBuilder => messageFormattingBuilder.SetPrefix((in LoggingTemplateEntry _) => $"{AnsiEscapeCodeConstants.DarkGrayForegroundColor}{DateTime.Now:yyyy-MM-dd HH:mm:ss}{AnsiEscapeCodeConstants.Reset} ")));
                });
            })
            .Build();
    }
}