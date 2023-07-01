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

using System.Diagnostics.CodeAnalysis;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

public interface IMediator
{
    [RequiresDynamicCode("The native code for this instantiation might not be available at runtime.")]
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    [RequiresDynamicCode("The native code for this instantiation might not be available at runtime.")]
    Task SendAsync(IRequest request, CancellationToken cancellationToken = default);

    Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification;
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