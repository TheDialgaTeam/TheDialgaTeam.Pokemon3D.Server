using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Middlewares;

internal sealed partial class LogSlowRequestMiddleware<TRequest> : IRequestMiddleware<TRequest> where TRequest : IRequest
{
    private readonly ILogger<LogSlowRequestMiddleware<TRequest>> _logger;

    public LogSlowRequestMiddleware(ILogger<LogSlowRequestMiddleware<TRequest>> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();
        await next().ConfigureAwait(false);
        var duration = Stopwatch.GetElapsedTime(startTime);

        if (duration.TotalMilliseconds > 500)
        {
            PrintLongRunningRequest(typeof(TRequest).Name, duration.TotalMilliseconds, request);
        }
    }
    
    [LoggerMessage(Level = LogLevel.Warning, Message = "[Request] Long Running Request: {name} ({elapsedMilliseconds:.2f} ms) {request}")]
    private partial void PrintLongRunningRequest(string name, double elapsedMilliseconds, TRequest request);
}

internal sealed partial class LogSlowRequestMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LogSlowRequestMiddleware<TRequest, TResponse>> _logger;

    public LogSlowRequestMiddleware(ILogger<LogSlowRequestMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
    
    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();
        var response = await next().ConfigureAwait(false);
        var duration = Stopwatch.GetElapsedTime(startTime);

        if (duration.TotalMilliseconds > 500)
        {
            PrintLongRunningRequest(typeof(TRequest).Name, duration.TotalMilliseconds, request);
        }

        return response;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Request] Long Running Request: {name} ({elapsedMilliseconds:F2} ms) {request}")]
    private partial void PrintLongRunningRequest(string name, double elapsedMilliseconds, TRequest request);
}