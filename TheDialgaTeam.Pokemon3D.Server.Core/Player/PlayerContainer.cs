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
using Mediator;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Commands;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Queries;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Player;

internal sealed class PlayerContainer : 
    IQueryHandler<GetPlayerCount, int>,
    ICommandHandler<CreateNewPlayer>
{
    private readonly List<IPlayer> _players = new();
    private readonly object _playerLock = new();

    private int _nextRunningId;
    private readonly ConcurrentQueue<int> _runningIdQueue = new();
    
    public ValueTask<int> Handle(GetPlayerCount query, CancellationToken cancellationToken)
    {
        lock (_playerLock)
        {
            return ValueTask.FromResult(_players.Count);
        }
    }

    public ValueTask<Unit> Handle(CreateNewPlayer command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    private int GetNextRunningId()
    {
        return _runningIdQueue.TryDequeue(out var id) ? id : Interlocked.Increment(ref _nextRunningId);
    }
}