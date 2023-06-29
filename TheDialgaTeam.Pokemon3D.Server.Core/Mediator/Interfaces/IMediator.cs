namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

public interface IMediator
{
    Task<TResponse> FetchAsync<TResponse>(IRequest<TResponse> query, CancellationToken cancellationToken);
    
    Task PostAsync(IRequest command, CancellationToken cancellationToken);
}

internal interface IBaseMediator
{
    Task PostAsync(IRequest command, CancellationToken cancellationToken);
}

internal interface IBaseMediator<TResponse>
{
    Task<TResponse> FetchAsync(IRequest<TResponse> query, CancellationToken cancellationToken);
}