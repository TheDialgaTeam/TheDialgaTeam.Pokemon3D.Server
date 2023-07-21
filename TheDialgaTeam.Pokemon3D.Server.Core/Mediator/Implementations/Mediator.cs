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

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Implementations;

internal sealed partial class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, IBaseMediatorSender> _senders = new();
    private readonly ConcurrentDictionary<Type, IBaseMediatorPublisher> _publishers = new();
    private readonly Dictionary<Type, Type> _mediatorTypes = new();

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Initialize();
    }

    public Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        var handler = Unsafe.As<IMediatorSender>(_senders.GetOrAdd(request.GetType(), static (type, args) => Unsafe.As<IBaseMediatorSender>(args._serviceProvider.GetRequiredService(args._mediatorTypes[type])), (_serviceProvider, _mediatorTypes)));
        return handler.SendAsync(request, cancellationToken);
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handler = Unsafe.As<IMediatorSender<TResponse>>(_senders.GetOrAdd(request.GetType(), static (type, args) => Unsafe.As<IBaseMediatorSender>(args._serviceProvider.GetRequiredService(args._mediatorTypes[type])), (_serviceProvider, _mediatorTypes)));
        return handler.SendAsync(request, cancellationToken);
    }

    public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
    {
        var handler = Unsafe.As<IMediatorPublisher>(_publishers.GetOrAdd(notification.GetType(), static (type, args) => Unsafe.As<IBaseMediatorPublisher>(args._serviceProvider.GetRequiredService(args._mediatorTypes[type])), (_serviceProvider, _mediatorTypes)));
        return handler.PublishAsync(notification, cancellationToken);
    }
    
    private partial void Initialize();
}