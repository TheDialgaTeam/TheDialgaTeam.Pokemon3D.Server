// Pokemon 3D Server Client
// Copyright (C) 2026 Yong Jian Ming
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
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Client;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network;

/// <summary>
/// This class acts as a container to store and handle all incoming connection from network listener.
/// </summary>
/// <param name="factory"></param>
public class NetworkClientContainer(NetworkClientHandlerFactory factory)
{
    private readonly ConcurrentDictionary<INetworkClient, NetworkClientHandler> _clients = new();
    
    public void AddNewConnection(INetworkClient networkClient)
    {
        _clients.TryAdd(networkClient, factory.Create(networkClient));
    }

    public void RemoveConnection(INetworkClient networkClient)
    {
        if (_clients.TryRemove(networkClient, out var handler))
        {
            handler.Dispose();
        }
    }
}