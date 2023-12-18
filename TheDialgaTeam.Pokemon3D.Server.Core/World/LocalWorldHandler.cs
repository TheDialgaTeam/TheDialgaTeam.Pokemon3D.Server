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
using TheDialgaTeam.Pokemon3D.Server.Core.World.Commands;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.World;

public sealed partial class LocalWorldHandler(
    ILogger<LocalWorldHandler> logger,
    IStringLocalizer stringLocalizer,
    ILocalWorldFactory localWorldFactory) :
    ICommandHandler<StartLocalWorld>,
    ICommandHandler<StopLocalWorld>,
    ICommandHandler<CreateLocalWorld, ILocalWorld>,
    INotificationHandler<LocalWorldUpdate>,
    IDisposable
{
    private readonly ILogger _logger = logger;
    private readonly ILocalWorld _localWorld = localWorldFactory.CreateLocalWorld();

    public ValueTask<Unit> Handle(StartLocalWorld command, CancellationToken cancellationToken)
    {
        _localWorld.StartWorld();
        return Unit.ValueTask;
    }

    public ValueTask<Unit> Handle(StopLocalWorld command, CancellationToken cancellationToken)
    {
        _localWorld.StopWorld();
        return Unit.ValueTask;
    }

    public ValueTask<ILocalWorld> Handle(CreateLocalWorld command, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(localWorldFactory.CreateLocalWorld(_localWorld, command.Player));
    }

    public ValueTask Handle(LocalWorldUpdate notification, CancellationToken cancellationToken)
    {
        var world = notification.LocalWorld;

        if (world.Player is null)
        {
            PrintInformation(stringLocalizer[s => s.ConsoleMessageFormat.GlobalWorldStatus, Enum.GetName(world.CurrentSeason), Enum.GetName(world.CurrentWeather), world.CurrentTime]);
        }
        else
        {
            world.Player.SendPacket(world.GetWorldDataPacket());
        }

        return ValueTask.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information, "[World] {Message}")]
    private partial void PrintInformation(string message);

    public void Dispose()
    {
        _localWorld.Dispose();
    }
}