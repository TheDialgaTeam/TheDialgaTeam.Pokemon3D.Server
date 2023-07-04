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
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Implementations;

internal sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, IBaseMediatorSender> _senders = new();
    private readonly ConcurrentDictionary<Type, IBaseMediatorPublisher> _publishers = new();

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [RequiresDynamicCode("The native code for this instantiation might not be available at runtime.")]
    public Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        var handler = (IMediatorSender) _senders.GetOrAdd(request.GetType(), static (type, serviceProvider) => (IBaseMediatorSender) serviceProvider.GetRequiredService(typeof(MediatorSender<>).MakeGenericType(type)), _serviceProvider);
        return handler.SendAsync(request, cancellationToken);
    }

    [RequiresDynamicCode("The native code for this instantiation might not be available at runtime.")]
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handler = (IMediatorSender<TResponse>) _senders.GetOrAdd(request.GetType(), static (type, serviceProvider) => (IBaseMediatorSender) serviceProvider.GetRequiredService(typeof(MediatorSender<,>).MakeGenericType(type, typeof(TResponse))), _serviceProvider);
        return handler.SendAsync(request, cancellationToken);
    }

    [RequiresDynamicCode("The native code for this instantiation might not be available at runtime.")]
    public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
    {
        var handler = (IMediatorPublisher) _publishers.GetOrAdd(notification.GetType(), static (type, serviceProvider) => (IBaseMediatorPublisher) serviceProvider.GetRequiredService(typeof(MediatorPublisher<>).MakeGenericType(type)), _serviceProvider);
        return handler.PublishAsync(notification, cancellationToken);
    }
}