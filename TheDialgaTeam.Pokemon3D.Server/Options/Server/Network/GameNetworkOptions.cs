using System.Net;

namespace TheDialgaTeam.Pokemon3D.Server.Options.Server.Network
{
    internal class GameNetworkOptions
    {
        public string BindIpAddress { get; set; } = IPAddress.Any.ToString();

        public int Port { get; set; } = 15124;

        public int NoPingKickTime { get; set; } = 10000;

        public int MaxPingAllowed { get; set; } = 1000;
    }
}