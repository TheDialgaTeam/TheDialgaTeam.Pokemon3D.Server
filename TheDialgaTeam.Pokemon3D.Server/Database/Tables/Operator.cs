using TheDialgaTeam.Pokemon3D.Server.Players;

namespace TheDialgaTeam.Pokemon3D.Server.Database.Tables
{
    internal class Operator
    {
        public string Name { get; set; } = string.Empty;

        public string GameJoltId { get; set; } = string.Empty;

        public PlayerType Level { get; set; }
    }
}