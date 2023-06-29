namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

public interface IBaseRequest
{
}

public interface IRequest : IBaseRequest
{
}

public interface IRequest<out TResponse> : IBaseRequest
{
}