namespace TheDialgaTeam.Mediator.Interfaces;

public interface IBaseRequest
{
}

public interface IRequest : IBaseRequest
{
}

public interface IRequest<out TResponse> : IBaseRequest
{
}