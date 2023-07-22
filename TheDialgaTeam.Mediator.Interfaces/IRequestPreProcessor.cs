namespace TheDialgaTeam.Mediator.Interfaces;

public interface IRequestPreProcessor<in TRequest> where TRequest : IBaseRequest
{
    Task Process(TRequest request, CancellationToken cancellationToken);
}