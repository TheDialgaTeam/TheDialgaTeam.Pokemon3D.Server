using System.Net;

namespace TheDialgaTeam.Pokemon3D.Server.Options.Server.Network
{
    internal class RpcNetworkOptions
    {
        public string BindIpAddress { get; set; } = IPAddress.Loopback.ToString();

        public int Port { get; set; } = 15125;

        public string Password { get; set; } = string.Empty;
    }
}