namespace TheDialgaTeam.Pokemon3D.Server.Core.Queries.Interfaces;

public interface IQueryHandler
{
}

public interface IQueryHandler<in TQuery, TQueryResult> : IQueryHandler where TQuery : IQuery<TQueryResult>
{
    Task<TQueryResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}