namespace TheDialgaTeam.Mediator.Abstractions;

public interface IRequestPreProcessor<in TRequest> where TRequest : IBaseRequest
{
    Task Process(TRequest request, CancellationToken cancellationToken);
}