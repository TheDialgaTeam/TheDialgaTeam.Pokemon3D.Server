namespace TheDialgaTeam.Pokemon3D.Server.Core.Queries.Interfaces;

public interface IQueryDispatcher
{
    Task<TQueryResult> DispatchAsync<TQueryResult>(IQuery<TQueryResult> query, CancellationToken cancellationToken);
}