using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Queries.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Queries.Implementations.Middleware;

internal sealed partial class LogSlowQueryMiddleware<TQuery, TQueryResult> : IQueryMiddleware<TQuery, TQueryResult> where TQuery : IQuery<TQueryResult>
{
    private readonly ILogger<LogSlowQueryMiddleware<TQuery, TQueryResult>> _logger;

    public LogSlowQueryMiddleware(ILogger<LogSlowQueryMiddleware<TQuery, TQueryResult>> logger)
    {
        _logger = logger;
    }
    
    public async Task<TQueryResult> HandleAsync(TQuery query, Func<Task<TQueryResult>> next, CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();
        var response = await next().ConfigureAwait(false);
        var duration = Stopwatch.GetElapsedTime(startTime);

        if (duration.TotalMilliseconds > 500)
        {
            PrintLongRunningRequest(typeof(TQuery).Name, duration.TotalMilliseconds, query);
        }

        return response;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Query] Long Running Request: {name} ({elapsedMilliseconds} ms) {query}")]
    private partial void PrintLongRunningRequest(string name, double elapsedMilliseconds, TQuery query);
}