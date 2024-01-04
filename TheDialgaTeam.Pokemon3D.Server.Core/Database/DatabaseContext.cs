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
using TheDialgaTeam.Pokemon3D.Server.Core.Database.Entities;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Database;

public sealed class DatabaseContext(IPokemonServerOptions options) : DbContext
{
    public required DbSet<PlayerProfile> PlayerProfiles { get; init; }

    public required DbSet<BannedPlayerProfile> BannedPlayerProfiles { get; init; }
    
    public required DbSet<BlockedPlayerProfile> BlockedPlayerProfiles { get; init; }
    
    public required DbSet<LocalWorld> LocalWorlds { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<PlayerProfile>()
            .HasOne<LocalWorld>(profile => profile.LocalWorld)
            .WithOne()
            .HasForeignKey<LocalWorld>(world => world.PlayerProfileId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder
            .Entity<PlayerProfile>()
            .HasOne<BannedPlayerProfile>()
            .WithOne()
            .HasForeignKey<BannedPlayerProfile>(profile => profile.PlayerProfileId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<PlayerProfile>()
            .HasMany<BlockedPlayerProfile>(profile => profile.BlockProfiles)
            .WithOne()
            .HasForeignKey(profile => profile.PlayerProfileId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<BlockedPlayerProfile>()
            .HasOne<PlayerProfile>()
            .WithMany()
            .HasForeignKey(profile => profile.BlockedProfileId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (options.DatabaseOptions.DatabaseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder
            {
                DataSource = options.DatabaseOptions.Sqlite.DataSource,
                Mode = options.DatabaseOptions.Sqlite.Mode,
                Cache = options.DatabaseOptions.Sqlite.Cache,
                Password = options.DatabaseOptions.Sqlite.Password,
                ForeignKeys = options.DatabaseOptions.Sqlite.ForeignKeys,
                RecursiveTriggers = options.DatabaseOptions.Sqlite.RecursiveTriggers,
                DefaultTimeout = options.DatabaseOptions.Sqlite.DefaultTimeout,
                Pooling = options.DatabaseOptions.Sqlite.Pooling
            }.ToString());
        }
    }
}