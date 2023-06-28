namespace TheDialgaTeam.Pokemon3D.Server.Core.Queries.Interfaces;

public interface IQueryMiddleware
{
}

public interface IQueryMiddleware<in TQuery, TQueryResult> : IQueryMiddleware where TQuery : IQuery<TQueryResult>
{
    Task<TQueryResult> HandleAsync(TQuery query, Func<Task<TQueryResult>> next, CancellationToken cancellationToken);
}