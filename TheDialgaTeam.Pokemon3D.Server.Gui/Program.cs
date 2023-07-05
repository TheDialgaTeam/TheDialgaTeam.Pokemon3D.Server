using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Hosting;
using TheDialgaTeam.Microsoft.Extensions.Logging;
using TheDialgaTeam.Microsoft.Extensions.Logging.Action;
using TheDialgaTeam.Pokemon3D.Server.Core.Extensions;

namespace TheDialgaTeam.Pokemon3D.Server.Gui;

public static class Program
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public static IServiceProvider ServiceProvider { get; private set; } = CreateHost(Array.Empty<string>()).Services;
    
    [RequiresDynamicCode("Binding strongly typed objects to configuration values may require generating dynamic code at runtime.")]
    [RequiresUnreferencedCode("Dependent types may have their members trimmed. Ensure all required members are preserved.")]
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

    [RequiresDynamicCode("Binding strongly typed objects to configuration values may require generating dynamic code at runtime.")]
    [RequiresUnreferencedCode("Dependent types may have their members trimmed. Ensure all required members are preserved.")]
    private static IHost CreateHost(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices(static collection =>
            {
                collection.AddPokemonServer();
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