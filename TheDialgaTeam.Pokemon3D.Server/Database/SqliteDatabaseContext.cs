#pragma warning disable 8618

using Microsoft.EntityFrameworkCore;
using TheDialgaTeam.Pokemon3D.Server.Database.Tables;

namespace TheDialgaTeam.Pokemon3D.Server.Database
{
    internal class SqliteDatabaseContext : DbContext
    {
        public DbSet<Blacklist> Blacklists { get; set; }

        public DbSet<Mutelist> Mutelists { get; set; }

        public DbSet<Operator> Operators { get; set; }

        public SqliteDatabaseContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blacklist>().HasKey(x => new { x.Name, x.GameJoltId });
            modelBuilder.Entity<Mutelist>().HasKey(x => new { x.Name, x.GameJoltId });
            modelBuilder.Entity<Operator>().HasKey(x => new { x.Name, x.GameJoltId });
        }
    }
}