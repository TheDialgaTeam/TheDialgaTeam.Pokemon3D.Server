namespace TheDialgaTeam.Mediator.Interfaces;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}