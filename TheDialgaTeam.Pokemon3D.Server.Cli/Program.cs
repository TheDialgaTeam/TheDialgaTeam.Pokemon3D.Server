// Pokemon 3D Server Client
// Copyright (C) 2026 Yong Jian Ming
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ReactiveUI.Builder;
using Terminal.Gui.App;
using TheDialgaTeam.Pokemon3D.Server.Cli.Scheduler;
using TheDialgaTeam.Pokemon3D.Server.Cli.Services;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Extensions;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Extensions;
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
            .ConfigurePokemonServerApplication()
            .ConfigurePokemonServerInfrastructure()
            .ConfigureServices(static (context, collection) =>
            {
                collection.AddMediator();

                if (bool.Parse(context.Configuration["GuiMode"] ?? "false"))
                {
                    collection.TryAddSingleton<ActionSinkOptions>();
                    collection.TryAddSingleton<IApplication>(_ => Application.Create());
                    collection.AddHostedService<ConsoleGuiService>();
                    collection.AddHostedService<ServerHostedService>();
                    collection.TryAddSingleton(provider =>
                        RxAppBuilder.CreateReactiveUIBuilder()
                            .WithMainThreadScheduler(new TerminalScheduler(provider.GetRequiredService<IApplication>()))
                            .BuildApp()
                    );
                }
                else
                {
                    collection.AddHostedService<ConsoleBackgroundService>();
                    collection.AddHostedService<ServerHostedService>();
                }
            })
            .ConfigureSerilog(static (context, provider, configuration) =>
            {
                if (bool.Parse(context.Configuration["GuiMode"] ?? "false"))
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