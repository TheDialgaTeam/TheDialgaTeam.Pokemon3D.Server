using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Core;
using TheDialgaTeam.Pokemon3D.Server.Options.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Serilog
{
    internal class Logger : IDisposable
    {
        private const string DateTimeTemplate = "\u001b[30;1m{DateTimeOffset:yyyy-MM-dd HH:mm:ss}\u001b[0m";

        private readonly ILogger<Logger> _logger;
        private readonly IDisposable _optionsDisposable;

        public Logger(ILogger<Logger> logger, LoggingLevelSwitch loggingLevelSwitch, IOptionsMonitor<SerilogOptions> optionsMonitor)
        {
            _logger = logger;
            _optionsDisposable = optionsMonitor.OnChange(options => { loggingLevelSwitch.MinimumLevel = options.Default; });

            loggingLevelSwitch.MinimumLevel = optionsMonitor.CurrentValue.Default;
        }

        public void LogTrace(string message, bool includeDateTime, params object[] args)
        {
            if (includeDateTime)
            {
                var newArgs = new object[args.Length + 1];
                newArgs[0] = DateTimeOffset.Now;
                Array.Copy(args, 0, newArgs, 1, args.Length);

                _logger.LogTrace($"{DateTimeTemplate} {message}", newArgs);
            }
            else
            {
                _logger.LogTrace(message, args);
            }
        }

        public void LogDebug(string message, bool includeDateTime, params object[] args)
        {
            if (includeDateTime)
            {
                var newArgs = new object[args.Length + 1];
                newArgs[0] = DateTimeOffset.Now;
                Array.Copy(args, 0, newArgs, 1, args.Length);

                _logger.LogDebug($"{DateTimeTemplate} {message}", newArgs);
            }
            else
            {
                _logger.LogDebug(message, args);
            }
        }

        public void LogInformation(string message, bool includeDateTime, params object[] args)
        {
            if (includeDateTime)
            {
                var newArgs = new object[args.Length + 1];
                newArgs[0] = DateTimeOffset.Now;
                Array.Copy(args, 0, newArgs, 1, args.Length);

                _logger.LogInformation($"{DateTimeTemplate} {message}", newArgs);
            }
            else
            {
                _logger.LogInformation(message, args);
            }
        }

        public void LogWarning(string message, bool includeDateTime, params object[] args)
        {
            if (includeDateTime)
            {
                var newArgs = new object[args.Length + 1];
                newArgs[0] = DateTimeOffset.Now;
                Array.Copy(args, 0, newArgs, 1, args.Length);

                _logger.LogWarning($"{DateTimeTemplate} \u001b[33;1m{message}\u001b[0m", newArgs);
            }
            else
            {
                _logger.LogWarning($"\u001b[33;1m{message}\u001b[0m", args);
            }
        }

        public void LogError(Exception exception, string message, bool includeDateTime, params object[] args)
        {
            if (includeDateTime)
            {
                var newArgs = new object[args.Length + 1];
                newArgs[0] = DateTimeOffset.Now;
                Array.Copy(args, 0, newArgs, 1, args.Length);

                _logger.LogError(exception, $"{DateTimeTemplate} \u001b[31;1m{message}\u001b[0m", newArgs);
            }
            else
            {
                _logger.LogError(exception, $"\u001b[31;1m{message}\u001b[0m", args);
            }
        }

        public void LogCritical(Exception exception, string message, bool includeDateTime, params object[] args)
        {
            if (includeDateTime)
            {
                var newArgs = new object[args.Length + 1];
                newArgs[0] = DateTimeOffset.Now;
                Array.Copy(args, 0, newArgs, 1, args.Length);

                _logger.LogCritical(exception, $"{DateTimeTemplate} \u001b[31;1m{message}\u001b[0m", newArgs);
            }
            else
            {
                _logger.LogCritical(exception, $"\u001b[31;1m{message}\u001b[0m", args);
            }
        }

        public void Dispose()
        {
            _optionsDisposable.Dispose();
        }
    }
}