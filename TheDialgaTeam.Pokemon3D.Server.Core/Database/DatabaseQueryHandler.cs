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
using TheDialgaTeam.Pokemon3D.Server.Core.Database.Entities;
using TheDialgaTeam.Pokemon3D.Server.Core.Database.Queries;
using TheDialgaTeam.Pokemon3D.Server.Core.Player;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Database;

public sealed class DatabaseQueryHandler :
    IQueryHandler<GetPlayerProfile, PlayerProfile?>,
    IDisposable, IAsyncDisposable
{
    private readonly DatabaseContext _context;
    
    public DatabaseQueryHandler(IDbContextFactory<DatabaseContext> contextFactory)
    {
        _context = contextFactory.CreateDbContext(); 
        _context.Database.Migrate();
    }

    public async ValueTask<PlayerProfile?> Handle(GetPlayerProfile query, CancellationToken cancellationToken)
    {
        var player = query.Player;
        
        if (player.IsGameJoltPlayer)
        {
            var playerProfile = await _context.PlayerProfiles.SingleOrDefaultAsync(profile => profile.GameJoltId == player.GameJoltId, cancellationToken).ConfigureAwait(false);

            if (playerProfile is not null)
            {
                return playerProfile;
            }
            
            // Check if there is any reserved names.
            var isNameConflict = await _context.PlayerProfiles.AnyAsync(profile => profile.DisplayName == player.Name, cancellationToken).ConfigureAwait(false);

            if (isNameConflict)
            {
                return null;
            }
            
            // If there is no conflict, make this name reserved.
            playerProfile = new PlayerProfile
            {
                DisplayName = player.Name, 
                GameJoltId = player.GameJoltId, 
                PlayerType = PlayerType.Player,
                LocalWorld = new LocalWorld()
            };

            _context.Add(playerProfile);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return playerProfile;
        }

        return null;
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync().ConfigureAwait(false);
    }
}