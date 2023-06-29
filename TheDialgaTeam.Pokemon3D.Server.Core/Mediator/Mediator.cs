using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator;

internal sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, IBaseMediatorSender> _caches = new();

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handler = (IMediatorSender<TResponse>) _caches.GetOrAdd(request.GetType(), (IBaseMediatorSender) _serviceProvider.GetRequiredService(typeof(MediatorSender<,>).MakeGenericType(request.GetType(), typeof(TResponse))));
        return handler.SendAsync(request, cancellationToken);
    }

    public Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        var handler = (IMediatorSender) _caches.GetOrAdd(request.GetType(), (IBaseMediatorSender) _serviceProvider.GetRequiredService(typeof(MediatorSender<>).MakeGenericType(request.GetType())));
        return handler.SendAsync(request, cancellationToken);
    }
}

internal sealed class MediatorSender<TRequest, TResponse> : IMediatorSender<TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IRequestHandler<TRequest, TResponse> _handler;
    private readonly IEnumerable<IRequestMiddleware<TRequest, TResponse>>? _middlewares;
    
    public MediatorSender(IRequestHandler<TRequest, TResponse> handler, IEnumerable<IRequestMiddleware<TRequest, TResponse>>? middlewares)
    {
        _handler = handler;
        _middlewares = middlewares;
    }

    public Task<TResponse> SendAsync(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        return SendAsync((TRequest) request, cancellationToken);
    }

    private Task<TResponse> SendAsync(TRequest request, CancellationToken cancellationToken)
    {
        return _middlewares == null ? _handler.HandleAsync(request, cancellationToken) : _middlewares.Aggregate(() => _handler.HandleAsync(request, cancellationToken), (next, middleware) => () => middleware.HandleAsync(request, next, cancellationToken))();
    }
}

internal sealed class MediatorSender<TRequest> : IMediatorSender where TRequest : IRequest
{
    private readonly IRequestHandler<TRequest> _handler;
    private readonly IEnumerable<IRequestMiddleware<TRequest>>? _middlewares;

    public MediatorSender(IRequestHandler<TRequest> handler, IEnumerable<IRequestMiddleware<TRequest>>? middlewares)
    {
        _handler = handler;
        _middlewares = middlewares;
    }

    public Task SendAsync(IRequest request, CancellationToken cancellationToken)
    {
        return SendAsync((TRequest) request, cancellationToken);
    }
    
    private Task SendAsync(TRequest request, CancellationToken cancellationToken)
    {
        return _middlewares == null ? _handler.HandleAsync(request, cancellationToken) : _middlewares.Aggregate(() => _handler.HandleAsync(request, cancellationToken), (next, middleware) => () => middleware.HandleAsync(request, next, cancellationToken))();
    }
}