namespace TheDialgaTeam.Mediator.Interfaces;

public interface IRequestPipeline
{
}

public interface IRequestPipeline<in TRequest> : IRequestPipeline where TRequest : IRequest
{
    Task HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken);
}

public interface IRequestPipeline<in TRequest, TResponse> : IRequestPipeline where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken);
}