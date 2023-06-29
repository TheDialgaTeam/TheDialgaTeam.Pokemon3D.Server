using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Middlewares;

internal sealed partial class CatchUnhandledExceptionMiddleware<TRequest> : IRequestMiddleware<TRequest> where TRequest : IRequest
{
    private readonly ILogger<CatchUnhandledExceptionMiddleware<TRequest>> _logger;

    public CatchUnhandledExceptionMiddleware(ILogger<CatchUnhandledExceptionMiddleware<TRequest>> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken)
    {
        try
        {
            return next();
        }
        catch (Exception exception)
        {
            PrintException(exception, typeof(TRequest).Name, request);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "[Request] Unhandled Exception for Request {name}: {request}")]
    private partial void PrintException(Exception exception, string name, TRequest request);
}

internal sealed partial class CatchUnhandledExceptionMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<CatchUnhandledExceptionMiddleware<TRequest, TResponse>> _logger;

    public CatchUnhandledExceptionMiddleware(ILogger<CatchUnhandledExceptionMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        try
        {
            return next();
        }
        catch (Exception exception)
        {
            PrintException(exception, typeof(TRequest).Name, request);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "[Request] Unhandled Exception for Request {name}: {request}")]
    private partial void PrintException(Exception exception, string name, TRequest request);
}