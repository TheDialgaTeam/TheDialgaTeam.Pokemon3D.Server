using TheDialgaTeam.Pokemon3D.Server.Options.Server.Features;

namespace TheDialgaTeam.Pokemon3D.Server.Options.Server
{
    internal class FeaturesOptions
    {
        public ChatOptions Chat { get; set; } = new();

        public PvPOptions PvP { get; set; } = new();

        public TradeOptions Trade { get; set; } = new();
    }
}