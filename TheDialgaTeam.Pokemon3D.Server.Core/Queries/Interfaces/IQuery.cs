namespace TheDialgaTeam.Pokemon3D.Server.Core.Queries.Interfaces;

public interface IQuery
{
}

public interface IQuery<out TQueryResult> : IQuery
{
}