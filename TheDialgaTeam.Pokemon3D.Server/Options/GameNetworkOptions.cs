using System.Net;

namespace TheDialgaTeam.Pokemon3D.Server.Options
{
    internal class GameNetworkOptions
    {
        public string BindIpAddress { get; set; } = IPAddress.Any.ToString();

        public int Port { get; set; } = 15124;
    }
}