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
using Microsoft.EntityFrameworkCore;
using TheDialgaTeam.Pokemon3D.Server.Core.Database.Queries;
using TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables;
using TheDialgaTeam.Pokemon3D.Server.Core.Player;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Database;

public sealed class DatabaseQueryHandler : IQueryHandler<GetPlayerProfile, PlayerProfile>
{
    private readonly IDbContextFactory<DatabaseContext> _contextFactory;

    public DatabaseQueryHandler(IDbContextFactory<DatabaseContext> contextFactory)
    {
        _contextFactory = contextFactory;

        using var context = contextFactory.CreateDbContext();
        context.Database.Migrate();
    }

    public async ValueTask<PlayerProfile> Handle(GetPlayerProfile query, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        if (query.GameDataPacket.IsGameJoltPlayer)
        {
            var isExists = await context.PlayerProfiles.CountAsync(profile => profile.GameJoltId == query.GameDataPacket.GameJoltId, cancellationToken).ConfigureAwait(false) > 0;

            if (isExists)
            {
                return await context.PlayerProfiles.SingleAsync(profile => profile.GameJoltId == query.GameDataPacket.GameJoltId, cancellationToken).ConfigureAwait(false);
            }

            var playerProfile = new PlayerProfile { GameJoltId = query.GameDataPacket.GameJoltId, PlayerType = PlayerType.GameJoltPlayer };
            context.Add(playerProfile);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            
            return playerProfile;
        }
        else
        {
            var isExists = await context.PlayerProfiles.CountAsync(profile => profile.Name == query.GameDataPacket.Name, cancellationToken).ConfigureAwait(false) > 0;

            if (isExists)
            {
                return await context.PlayerProfiles.SingleAsync(profile => profile.Name == query.GameDataPacket.Name, cancellationToken).ConfigureAwait(false);
            }

            var playerProfile = new PlayerProfile { Name = query.GameDataPacket.Name, PlayerType = PlayerType.OfflinePlayer };
            context.Add(playerProfile);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            
            return playerProfile;
        }
    }
}