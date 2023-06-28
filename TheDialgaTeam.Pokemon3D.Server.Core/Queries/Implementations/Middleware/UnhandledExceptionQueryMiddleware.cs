using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Queries.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Queries.Implementations.Middleware;

internal sealed partial class UnhandledExceptionQueryMiddleware<TQuery, TQueryResult> : IQueryMiddleware<TQuery, TQueryResult> where TQuery : IQuery<TQueryResult>
{
    private readonly ILogger<UnhandledExceptionQueryMiddleware<TQuery, TQueryResult>> _logger;

    public UnhandledExceptionQueryMiddleware(ILogger<UnhandledExceptionQueryMiddleware<TQuery, TQueryResult>> logger)
    {
        _logger = logger;
    }

    public Task<TQueryResult> HandleAsync(TQuery query, Func<Task<TQueryResult>> next, CancellationToken cancellationToken)
    {
        try
        {
            return next();
        }
        catch (Exception exception)
        {
            PrintQueryException(exception, typeof(TQuery).Name, query);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "[Query] Unhandled Exception for Request {name} {query}")]
    private partial void PrintQueryException(Exception exception, string name, TQuery query);
}