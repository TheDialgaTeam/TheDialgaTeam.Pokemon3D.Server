namespace TheDialgaTeam.Mediator.Abstractions;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}