namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

public interface IRequestHandler
{
}

public interface IRequestHandler<in TRequest> : IRequestHandler where TRequest : IRequest
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public interface IRequestHandler<in TRequest, TResponse> : IRequestHandler where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}