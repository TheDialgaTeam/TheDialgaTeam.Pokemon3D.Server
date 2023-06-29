namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

public interface IRequestMiddleware
{
}

public interface IRequestMiddleware<in TRequest> : IRequestMiddleware where TRequest : IRequest
{
    Task HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken);
}

public interface IRequestMiddleware<in TRequest, TResponse> : IRequestMiddleware where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken);
}