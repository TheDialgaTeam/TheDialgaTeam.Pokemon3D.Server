using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Filters;
using TheDialgaTeam.Core.Logger;
using TheDialgaTeam.Core.Logger.Formatter;
using TheDialgaTeam.Pokemon3D.Server.Network;
using TheDialgaTeam.Pokemon3D.Server.Network.Game;
using TheDialgaTeam.Pokemon3D.Server.Options;
using TheDialgaTeam.Pokemon3D.Server.Serilog;
using TheDialgaTeam.Pokemon3D.Server.Services;
using TheDialgaTeam.Pokemon3D.Server.ViewModels;
using TheDialgaTeam.Pokemon3D.Server.Views;
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
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostBuilderContext, serviceCollection) =>
                {
                    var configuration = hostBuilderContext.Configuration;

                    serviceCollection.Configure<ApplicationOptions>(configuration.GetSection("Application"));

                    serviceCollection.Configure<NetworkOptions>(configuration.GetSection("Server:Network"));
                    serviceCollection.Configure<GameNetworkOptions>(configuration.GetSection("Server:Network:Game"));
                    serviceCollection.Configure<RpcNetworkOptions>(configuration.GetSection("Server:Network:Rpc"));

                    serviceCollection.AddSingleton<LoggingLevelSwitch>();
                    serviceCollection.AddSingleton<Logger>();

                    serviceCollection.AddSingleton<NatDevices>();
                    serviceCollection.AddSingleton<TcpClientListener>();
                    serviceCollection.AddSingleton<Server>();

                    if (configuration.GetValue("Application:HeadlessMode", true)) return;

                    serviceCollection.AddSingleton(_ => new ClassicDesktopStyleApplicationLifetime
                    {
                        Args = args,
                        ShutdownMode = ShutdownMode.OnMainWindowClose
                    });

                    serviceCollection.AddSingleton<UiLogger>();

                    serviceCollection.AddHostedService<AvaloniaHostedService>();
                    serviceCollection.AddSingleton<MainWindowViewModel>();
                })
                .UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
                {
                    const string outputTemplate = "{Message}{NewLine}{Exception}";

                    var headlessMode = hostBuilderContext.Configuration.GetValue("Application:HeadlessMode", true);
                    var outputTemplateTextFormatter = new OutputTemplateTextFormatter(outputTemplate);

                    loggerConfiguration
                        .ReadFrom.Configuration(hostBuilderContext.Configuration)
                        .MinimumLevel.ControlledBy(serviceProvider.GetRequiredService<LoggingLevelSwitch>())
                        .WriteTo.Conditional(_ => headlessMode, loggerSinkConfiguration =>
                        {
                            loggerSinkConfiguration.AnsiConsole(outputTemplateTextFormatter);
                        })
                        .WriteTo.Conditional(_ => !headlessMode, loggerSinkConfiguration =>
                        {
                            loggerSinkConfiguration.Logger(configuration => configuration
                                .Filter.ByExcluding(Matching.FromSource<Logger>())
                                .WriteTo.AnsiConsole(outputTemplateTextFormatter));

                            loggerSinkConfiguration.Logger(configuration => configuration
                                .Filter.ByIncludingOnly(Matching.FromSource<Logger>())
                                .WriteTo.UiConsole(serviceProvider.GetRequiredService<UiLogger>().WriteToLogOutput, outputTemplateTextFormatter));
                        });
                });
        }
    }
}