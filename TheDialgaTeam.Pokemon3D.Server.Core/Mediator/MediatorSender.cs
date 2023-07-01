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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator;

internal sealed class MediatorSender<TRequest, TResponse> : IMediatorSender<TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IRequestHandler<TRequest, TResponse> _handler;
    private readonly IEnumerable<IRequestMiddleware<TRequest, TResponse>>? _middlewares;

    public MediatorSender(IRequestHandler<TRequest, TResponse> handler)
    {
        _handler = handler;
        _middlewares = null;
    }

    public MediatorSender(IRequestHandler<TRequest, TResponse> handler, IEnumerable<IRequestMiddleware<TRequest, TResponse>> middlewares)
    {
        _handler = handler;
        _middlewares = middlewares;
    }

    public Task<TResponse> SendAsync(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        return SendAsync((TRequest) request, cancellationToken);
    }

    private Task<TResponse> SendAsync(TRequest request, CancellationToken cancellationToken)
    {
        return _middlewares == null ? _handler.HandleAsync(request, cancellationToken) : _middlewares.Aggregate(() => _handler.HandleAsync(request, cancellationToken), (next, middleware) => () => middleware.HandleAsync(request, next, cancellationToken))();
    }
}

internal sealed class MediatorSender<TRequest> : IMediatorSender where TRequest : IRequest
{
    private readonly IRequestHandler<TRequest> _handler;
    private readonly IEnumerable<IRequestMiddleware<TRequest>>? _middlewares;

    public MediatorSender(IRequestHandler<TRequest> handler)
    {
        _handler = handler;
        _middlewares = null;
    }

    public MediatorSender(IRequestHandler<TRequest> handler, IEnumerable<IRequestMiddleware<TRequest>> middlewares)
    {
        _handler = handler;
        _middlewares = middlewares;
    }

    public Task SendAsync(IRequest request, CancellationToken cancellationToken)
    {
        return SendAsync((TRequest) request, cancellationToken);
    }

    private Task SendAsync(TRequest request, CancellationToken cancellationToken)
    {
        return _middlewares == null ? _handler.HandleAsync(request, cancellationToken) : _middlewares.Aggregate(() => _handler.HandleAsync(request, cancellationToken), (next, middleware) => () => middleware.HandleAsync(request, next, cancellationToken))();
    }
}