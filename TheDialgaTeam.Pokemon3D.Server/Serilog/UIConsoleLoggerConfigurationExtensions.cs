using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using TheDialgaTeam.Pokemon3D.Server.Serilog.Sink;

namespace TheDialgaTeam.Pokemon3D.Server.Serilog
{
    internal static class UiConsoleLoggerConfigurationExtensions
    {
        private static readonly object DefaultSyncRoot = new();

        public static LoggerConfiguration UiConsole(this LoggerSinkConfiguration sinkConfiguration, Action<string> outputAction, ITextFormatter formatter, LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum, LoggingLevelSwitch? levelSwitch = null, LogEventLevel standardErrorFromLevel = LogEventLevel.Error, object? syncRoot = null)
        {
            syncRoot ??= DefaultSyncRoot;
            return sinkConfiguration.Sink(new UiConsoleSink(outputAction, formatter, syncRoot), restrictedToMinimumLevel, levelSwitch);
        }
    }
}