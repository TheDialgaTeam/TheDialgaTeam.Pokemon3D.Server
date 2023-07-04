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

using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces.Alias;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

internal sealed class NetworkContainer : IEventHandler<ConnectedEventArgs>, IEventHandler<DisconnectedEventArgs>
{
    private readonly List<IClientNetwork> _tcpClientNetworks = new();
    private readonly object _clientLock = new();

    private void AddClient(IClientNetwork network)
    {
        lock (_clientLock)
        {
            _tcpClientNetworks.Add(network);
        }
    }

    private void RemoveClient(IClientNetwork network)
    {
        lock (_clientLock)
        {
            _tcpClientNetworks.Remove(network);
        }
    }
    
    public Task HandleAsync(ConnectedEventArgs e, CancellationToken cancellationToken)
    {
        AddClient(e.ClientNetwork);
        return Task.CompletedTask;
    }

    public Task HandleAsync(DisconnectedEventArgs e, CancellationToken cancellationToken)
    {
        e.ClientNetwork.Dispose();
        RemoveClient(e.ClientNetwork);
        return Task.CompletedTask;
    }
}