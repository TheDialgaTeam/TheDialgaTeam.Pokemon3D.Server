#pragma warning disable 8618

using Microsoft.EntityFrameworkCore;
using TheDialgaTeam.Pokemon3D.Server.Database.Tables;

namespace TheDialgaTeam.Pokemon3D.Server.Database
{
    internal class SqliteDatabaseContext : DbContext
    {
        public DbSet<Blacklist> Blacklists { get; }

        public DbSet<IPBlacklist> IpBlacklists { get; }

        public DbSet<MuteList> MuteLists { get; }

        public DbSet<OperatorList> OperatorLists { get; }

        public DbSet<Whitelist> Whitelists { get; }

        public SqliteDatabaseContext(DbContextOptions<SqliteDatabaseContext> contextOptions) : base(contextOptions)
        {
        }
    }
}