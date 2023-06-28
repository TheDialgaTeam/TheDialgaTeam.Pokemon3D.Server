using TheDialgaTeam.Pokemon3D.Server.Core.Queries.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Queries.Implementations;

internal class QueryDispatcherHelper<TQuery, TQueryResult> : IQueryDispatcherHelper<TQueryResult> where TQuery : IQuery<TQueryResult>
{
    private readonly IQueryHandler<TQuery, TQueryResult> _handler;
    private readonly IEnumerable<IQueryMiddleware<TQuery, TQueryResult>> _middlewares;

    public QueryDispatcherHelper(IQueryHandler<TQuery, TQueryResult> handler, IEnumerable<IQueryMiddleware<TQuery, TQueryResult>> middlewares)
    {
        _handler = handler;
        _middlewares = middlewares;
    }
    
    public Task<TQueryResult> DispatchAsync(IQuery<TQueryResult> query, CancellationToken cancellationToken)
    {
        return DispatchAsync((TQuery) query, cancellationToken);
    }

    private Task<TQueryResult> DispatchAsync(TQuery query, CancellationToken cancellationToken)
    {
        return _middlewares.Reverse().Aggregate(() => _handler.HandleAsync(query, cancellationToken), (next, middleware) => () => middleware.HandleAsync(query, next, cancellationToken))();
    }
}