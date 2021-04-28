#pragma warning disable 8618

using System.ComponentModel.DataAnnotations;

namespace TheDialgaTeam.Pokemon3D.Server.Database.Tables
{
    internal class Whitelist
    {
        [Key]
        public string Name { get; set; }

        public int GameJoltId { get; set; }
    }
}