namespace TheDialgaTeam.Mediator.Interfaces;

public interface IMediator
{
    Task SendAsync(IRequest request, CancellationToken cancellationToken = default);

    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    Task PublishAsync(INotification notification, CancellationToken cancellationToken = default);
}