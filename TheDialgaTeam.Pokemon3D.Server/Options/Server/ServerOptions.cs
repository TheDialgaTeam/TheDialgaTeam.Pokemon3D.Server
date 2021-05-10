namespace TheDialgaTeam.Pokemon3D.Server.Options.Server
{
    internal class ServerOptions
    {
        public NetworkOptions Network { get; set; } = new();

        public string ProtocolVersion { get; set; } = "0.5";

        public string ServerName { get; set; } = string.Empty;

        public string ServerDescription { get; set; } = string.Empty;

        public string WelcomeMessage { get; set; } = string.Empty;

        public string[] GameModes { get; set; } = { "Pokemon 3D" };

        public int MaxPlayers { get; set; } = 20;

        public bool OfflineMode { get; set; } = false;

        public int AFKKickTime { get; set; } = 300;

        public WorldOptions World { get; set; } = new();

        public FeaturesOptions Features { get; set; } = new();
    }
}