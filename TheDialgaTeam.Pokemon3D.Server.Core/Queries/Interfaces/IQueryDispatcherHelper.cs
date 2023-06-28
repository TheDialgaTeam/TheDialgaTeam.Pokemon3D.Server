namespace TheDialgaTeam.Pokemon3D.Server.Core.Queries.Interfaces;

internal interface IQueryDispatcherHelper
{
}

internal interface IQueryDispatcherHelper<TQueryResult> : IQueryDispatcherHelper
{
    Task<TQueryResult> DispatchAsync(IQuery<TQueryResult> query, CancellationToken cancellationToken);
}