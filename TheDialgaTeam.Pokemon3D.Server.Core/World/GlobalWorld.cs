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

using Mediator;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Queries;

namespace TheDialgaTeam.Pokemon3D.Server.Core.World;

public sealed class GlobalWorld : 
    IQueryHandler<GetGlobalWorld, ILocalWorld>,
    INotificationHandler<WorldUpdate>
{
    private readonly ILogger<GlobalWorld> _logger;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly ILocalWorld _world;

    public GlobalWorld(ILogger<GlobalWorld> logger, IStringLocalizer stringLocalizer, ILocalWorldFactory factory)
    {
        _logger = logger;
        _stringLocalizer = stringLocalizer;
        _world = factory.CreateLocalWorld();
    }

    public ValueTask<ILocalWorld> Handle(GetGlobalWorld query, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(_world);
    }

    public ValueTask Handle(WorldUpdate notification, CancellationToken cancellationToken)
    {
        if (notification.World == _world)
        {
            _logger.LogInformation("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.GlobalWorldStatus, Enum.GetName(notification.World.CurrentSeason), Enum.GetName(notification.World.CurrentWeather), notification.World.CurrentTime]);
        }
        
        return ValueTask.CompletedTask;
    }
}