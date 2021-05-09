namespace TheDialgaTeam.Pokemon3D.Server.Options.Server
{
    internal class NetworkOptions
    {
        public GameNetworkOptions Game { get; set; } = new();

        public RpcNetworkOptions Rpc { get; set; } = new();

        public bool UseUniversalPlugAndPlay { get; set; } = false;

        public bool UsePortMappingProtocol { get; set; } = false;
    }
}