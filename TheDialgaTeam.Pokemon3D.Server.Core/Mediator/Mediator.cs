using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator;

internal sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> FetchAsync<TResponse>(IRequest<TResponse> query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task PostAsync(IRequest command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}