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

using Mediator;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.World.Command;
using TheDialgaTeam.Pokemon3D.Server.Core.Domain.World;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Application.World;

public sealed class WorldService :
    ICommandHandler<StartGlobalWorld>,
    ICommandHandler<StopGlobalWorld>,
    IDisposable, IAsyncDisposable
{
    private readonly IMediator _mediator;
    private readonly Timer _globalWorldTimer;
    private readonly LocalWorld _globalWorld = new();

    public WorldService(IMediator mediator)
    {
        _mediator = mediator;
        _globalWorldTimer = new Timer(GlobalWorldUpdate, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public ValueTask<Unit> Handle(StartGlobalWorld command, CancellationToken cancellationToken)
    {
        _globalWorldTimer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));
        return Unit.ValueTask;
    }

    public ValueTask<Unit> Handle(StopGlobalWorld command, CancellationToken cancellationToken)
    {
        _globalWorldTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        return Unit.ValueTask;
    }

    private void GlobalWorldUpdate(object? state)
    {
        _globalWorld.UpdateLocalWorld(DateTimeOffset.Now);
    }

    public void Dispose()
    {
        _globalWorldTimer.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _globalWorldTimer.DisposeAsync().ConfigureAwait(false);
    }
}