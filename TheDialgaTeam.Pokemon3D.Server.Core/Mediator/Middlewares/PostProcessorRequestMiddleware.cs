// Pokemon 3D Server Client
// Copyright (C) 2023 Yong Jian Ming
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Middlewares;

internal sealed class PostProcessorRequestMiddleware<TRequest> : IRequestMiddleware<TRequest> where TRequest : IRequest
{
    private readonly IEnumerable<IRequestPostProcessor<TRequest>> _processors;

    public PostProcessorRequestMiddleware()
    {
        _processors = Array.Empty<IRequestPostProcessor<TRequest>>();
    }

    public PostProcessorRequestMiddleware(IEnumerable<IRequestPostProcessor<TRequest>> processors)
    {
        _processors = processors;
    }
    
    public async Task HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken)
    {
        await next().ConfigureAwait(false);
        
        foreach (var processor in _processors)
        {
            await processor.Process(request, cancellationToken).ConfigureAwait(false);
        }
    }
}

internal sealed class PostProcessorRequestMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IRequestPostProcessor<TRequest, TResponse>> _processors;

    public PostProcessorRequestMiddleware()
    {
        _processors = Array.Empty<IRequestPostProcessor<TRequest, TResponse>>();
    }

    public PostProcessorRequestMiddleware(IEnumerable<IRequestPostProcessor<TRequest, TResponse>> processors)
    {
        _processors = processors;
    }
    
    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        var response = await next().ConfigureAwait(false);
        
        foreach (var processor in _processors)
        {
            await processor.Process(request, response, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }
}