using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheDialgaTeam.Microsoft.Extensions.Logging;
using TheDialgaTeam.Microsoft.Extensions.Logging.AnsiConsole;
using TheDialgaTeam.Pokemon3D.Server.Core.Extensions;

namespace TheDialgaTeam.Pokemon3D.Server.Cli;

internal static class Program
{
    [RequiresDynamicCode("Binding strongly typed objects to configuration values may require generating dynamic code at runtime.")]
    [RequiresUnreferencedCode("Dependent types may have their members trimmed. Ensure all required members are preserved.")]
    public static Task Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainOnUnhandledException;

        return Host.CreateDefaultBuilder(args)
            .ConfigureServices(static collection =>
            {
                collection.AddPokemonServer();
                collection.AddMediator();
                collection.AddHostedService<ConsoleService>();
            })
            .ConfigureLogging(static builder =>
            {
                builder.AddAnsiConsole(static options =>
                {
                    options.SetDefaultTemplate(static formattingBuilder => formattingBuilder.SetGlobal(static messageFormattingBuilder => messageFormattingBuilder.SetPrefix(static (in LoggingTemplateEntry _) => $"{AnsiEscapeCodeConstants.DarkGrayForegroundColor}{DateTime.Now:yyyy-MM-dd HH:mm:ss}{AnsiEscapeCodeConstants.Reset} ")));
                });
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