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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Implementations;

internal sealed class MediatorPublisher<TNotification> : IMediatorPublisher where TNotification : INotification
{
    private readonly IEnumerable<INotificationHandler<TNotification>> _handlers;

    public MediatorPublisher() : this(Array.Empty<INotificationHandler<TNotification>>())
    {
    }

    public MediatorPublisher(IEnumerable<INotificationHandler<TNotification>> handlers)
    {
        _handlers = handlers;
    }

    public Task PublishAsync(INotification notification, CancellationToken cancellationToken)
    {
        return Task.WhenAll(_handlers.Select(handler => handler.HandleAsync((TNotification) notification, cancellationToken)));
    }
}