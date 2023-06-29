namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

public interface IMediator
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    
    Task SendAsync(IRequest request, CancellationToken cancellationToken = default);
}

internal interface IBaseMediatorSender
{
}

internal interface IMediatorSender : IBaseMediatorSender
{
    Task SendAsync(IRequest request, CancellationToken cancellationToken);
}

internal interface IMediatorSender<TResponse> : IBaseMediatorSender
{
    Task<TResponse> SendAsync(IRequest<TResponse> request, CancellationToken cancellationToken);
}