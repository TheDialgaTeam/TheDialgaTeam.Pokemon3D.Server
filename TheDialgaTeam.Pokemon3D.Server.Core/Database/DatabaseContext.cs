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

using Microsoft.EntityFrameworkCore;
using TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Database;

public sealed class DatabaseContext : DbContext
{
    private readonly IPokemonServerOptions _options;
    
    public required DbSet<PlayerProfile> PlayerProfiles { get; init; }
    
    public required DbSet<Blacklist> BlacklistAccounts { get; init; }
    
    public required DbSet<Whitelist> WhitelistAccounts { get; init; }
    
    public DatabaseContext(IPokemonServerOptions options)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_options.DatabaseOptions.UseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlite(_options.DatabaseOptions.Sqlite.ToString());
        }
    }
}