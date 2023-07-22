namespace TheDialgaTeam.Mediator.Interfaces;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}