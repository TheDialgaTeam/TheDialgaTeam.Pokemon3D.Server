using Avalonia;
using Avalonia.ReactiveUI;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
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
        ServiceProvider = CreateHost(args).Services;
        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUI();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026")]
    private static IHost CreateHost(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigurePokemonServer()
            .ConfigureServices(static collection => 
            {
                collection.AddMediator();
            })
            .ConfigureLogging(static builder =>
            {
                builder.AddActionLogger(static options =>
                {
                    options.SetDefaultTemplate(static formattingBuilder => formattingBuilder.SetGlobal(static messageFormattingBuilder => messageFormattingBuilder.SetPrefix(static (in LoggingTemplateEntry _) => $"{AnsiEscapeCodeConstants.DarkGrayForegroundColor}{DateTime.Now:yyyy-MM-dd HH:mm:ss}{AnsiEscapeCodeConstants.Reset} ")));
                });
            })
            .Build();
    }
}