namespace TheDialgaTeam.Mediator.Abstractions;

public interface IQueryHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IQuery<TResponse>
{
}