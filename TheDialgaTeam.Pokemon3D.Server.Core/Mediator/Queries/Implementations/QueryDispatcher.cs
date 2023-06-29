using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Queries.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Queries.Implementations;

internal sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, IQueryDispatcherHelper> _caches = new();

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TQueryResult> DispatchAsync<TQueryResult>(IQuery<TQueryResult> query, CancellationToken cancellationToken)
    {
        var handler = (IQueryDispatcherHelper<TQueryResult>) _caches.GetOrAdd(query.GetType(), (IQueryDispatcherHelper) _serviceProvider.GetRequiredService(typeof(QueryDispatcherHelper<,>).MakeGenericType(query.GetType(), typeof(TQueryResult))));
        return handler.DispatchAsync(query, cancellationToken);
    }
}

