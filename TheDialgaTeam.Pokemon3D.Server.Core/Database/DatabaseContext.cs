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

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Database;

public sealed class DatabaseContext : DbContext
{
    public required DbSet<PlayerProfile> PlayerProfiles { get; init; }

    public required DbSet<BannedPlayerProfile> BannedPlayerProfiles { get; init; }
    
    public required DbSet<BlockedPlayerProfile> BlockedPlayerProfiles { get; init; }
    
    public required DbSet<LocalWorld> LocalWorlds { get; init; }
    
    private readonly IPokemonServerOptions _options;

    public DatabaseContext(IPokemonServerOptions options)
    {
        _options = options;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<PlayerProfile>()
            .HasMany<BlockedPlayerProfile>(profile => profile.BlockProfiles)
            .WithOne(blocklist => blocklist.PlayerProfile)
            .HasForeignKey("PlayerProfileId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<PlayerProfile>()
            .HasOne<LocalWorld>(profile => profile.LocalWorld)
            .WithOne(world => world.PlayerProfile)
            .HasForeignKey<LocalWorld>("PlayerProfileId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<PlayerProfile>()
            .HasOne<BannedPlayerProfile>(profile => profile.Blacklist)
            .WithOne(blacklist => blacklist.PlayerProfile)
            .HasForeignKey<BannedPlayerProfile>("PlayerProfileId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_options.DatabaseOptions.DatabaseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder
            {
                DataSource = _options.DatabaseOptions.Sqlite.DataSource,
                Mode = _options.DatabaseOptions.Sqlite.Mode,
                Cache = _options.DatabaseOptions.Sqlite.Cache,
                Password = _options.DatabaseOptions.Sqlite.Password,
                ForeignKeys = _options.DatabaseOptions.Sqlite.ForeignKeys,
                RecursiveTriggers = _options.DatabaseOptions.Sqlite.RecursiveTriggers,
                DefaultTimeout = _options.DatabaseOptions.Sqlite.DefaultTimeout,
                Pooling = _options.DatabaseOptions.Sqlite.Pooling
            }.ToString());
        }
    }
}