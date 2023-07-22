namespace TheDialgaTeam.Mediator.Interfaces;

public interface IRequestPostProcessor<in TRequest> where TRequest : IBaseRequest
{
    Task Process(TRequest request, CancellationToken cancellationToken);
}

public interface IRequestPostProcessor<in TRequest, in TResponse> where TRequest : IBaseRequest
{
    Task Process(TRequest request, TResponse response, CancellationToken cancellationToken);
}