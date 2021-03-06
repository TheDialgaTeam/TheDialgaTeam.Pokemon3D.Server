using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Filters;
using TheDialgaTeam.Core.Logger;
using TheDialgaTeam.Core.Logger.Formatter;
using TheDialgaTeam.Pokemon3D.Server.Database;
using TheDialgaTeam.Pokemon3D.Server.Frontend.Console.ViewModels;
using TheDialgaTeam.Pokemon3D.Server.Frontend.Console.Views;
using TheDialgaTeam.Pokemon3D.Server.Frontend.GUI;
using TheDialgaTeam.Pokemon3D.Server.Frontend.GUI.ViewModels;
using TheDialgaTeam.Pokemon3D.Server.Network;
using TheDialgaTeam.Pokemon3D.Server.Options.Application;
using TheDialgaTeam.Pokemon3D.Server.Options.Serilog;
using TheDialgaTeam.Pokemon3D.Server.Options.Server;
using TheDialgaTeam.Pokemon3D.Server.Options.Server.Network;
using TheDialgaTeam.Pokemon3D.Server.Players;
using TheDialgaTeam.Pokemon3D.Server.Serilog;
using TheDialgaTeam.Pokemon3D.Server.Services;
using TheDialgaTeam.Pokemon3D.Server.Worlds;
using Logger = TheDialgaTeam.Pokemon3D.Server.Serilog.Logger;

namespace TheDialgaTeam.Pokemon3D.Server
{
    internal static class Program
    {
        public static IServiceProvider Container { get; private set; } = null!;

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI();
        }

        private static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            Container = host.Services;
            host.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .ConfigureServices((hostBuilderContext, serviceCollection) =>
                {
                    var configuration = hostBuilderContext.Configuration;

                    // Options
                    serviceCollection.Configure<SerilogOptions>(configuration.GetSection("Serilog:MinimumLevel"));
                    serviceCollection.Configure<ApplicationOptions>(configuration.GetSection("Application"));
                    serviceCollection.Configure<ServerOptions>(configuration.GetSection("Server"));
                    serviceCollection.Configure<NetworkOptions>(configuration.GetSection("Server:Network"));
                    serviceCollection.Configure<WorldOptions>(configuration.GetSection("Server:World"));

                    // Database
                    serviceCollection.AddDbContextFactory<SqliteDatabaseContext>(builder => { builder.UseSqlite($"Data Source={Path.Combine(hostBuilderContext.HostingEnvironment.ContentRootPath, "data.db")}"); });

                    // Logger
                    serviceCollection.AddSingleton<LoggingLevelSwitch>();
                    serviceCollection.AddSingleton<Logger>();

                    // Network
                    serviceCollection.AddSingleton<NatDevices>();
                    serviceCollection.AddSingleton<TcpClientListener>();
                    serviceCollection.AddSingleton<TcpClientCollection>();

                    // Player
                    serviceCollection.AddSingleton<PlayerCollection>();

                    // World
                    serviceCollection.AddSingleton<World>();

                    serviceCollection.AddSingleton<GameServer>();

                    var headlessMode = configuration.GetValue("Application:HeadlessMode", true);

                    if (headlessMode)
                    {
                        serviceCollection.AddSingleton<ConsoleWindow>();
                        serviceCollection.AddSingleton<ConsoleWindowViewModel>();

                        serviceCollection.AddHostedService<ConsoleHostedService>();
                    }
                    else
                    {
                        serviceCollection.AddSingleton(_ => new ClassicDesktopStyleApplicationLifetime
                        {
                            Args = args,
                            ShutdownMode = ShutdownMode.OnMainWindowClose
                        });
                        serviceCollection.AddSingleton<ActionLogger>();
                        serviceCollection.AddSingleton<MainWindowViewModel>();

                        serviceCollection.AddHostedService<AvaloniaHostedService>();
                    }
                })
                .UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
                {
                    var configuration = loggerConfiguration
                        .ReadFrom.Configuration(hostBuilderContext.Configuration)
                        .MinimumLevel.ControlledBy(serviceProvider.GetRequiredService<LoggingLevelSwitch>());

                    var headlessMode = hostBuilderContext.Configuration.GetValue("Application:HeadlessMode", true);
                    var outputTemplateTextFormatter = new OutputTemplateTextFormatter("{Message}{NewLine}{Exception}");

                    if (headlessMode)
                    {
                        configuration.WriteTo.AnsiConsole(outputTemplateTextFormatter);
                    }
                    else
                    {
                        configuration.WriteTo.Logger(subLogger => subLogger
                            .Filter.ByExcluding(Matching.FromSource<Logger>())
                            .WriteTo.AnsiConsole(outputTemplateTextFormatter));

                        configuration.WriteTo.Logger(subLogger => subLogger
                            .Filter.ByIncludingOnly(Matching.FromSource<Logger>())
                            .WriteTo.Action(serviceProvider.GetRequiredService<ActionLogger>().WriteToLogEvent, outputTemplateTextFormatter));
                    }
                });
        }
    }
}