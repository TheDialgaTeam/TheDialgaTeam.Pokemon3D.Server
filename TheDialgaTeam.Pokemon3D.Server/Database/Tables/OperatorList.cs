#pragma warning disable 8618

using System.ComponentModel.DataAnnotations;
using TheDialgaTeam.Pokemon3D.Server.Players;

namespace TheDialgaTeam.Pokemon3D.Server.Database.Tables
{
    internal class OperatorList
    {
        [Key]
        public string Name { get; set; }

        public int GameJoltId { get; set; }

        public PlayerType Level { get; set; }
    }
}