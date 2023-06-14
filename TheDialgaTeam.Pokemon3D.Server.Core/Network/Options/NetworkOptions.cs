using System.Net;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Options;

public sealed class NetworkOptions
{
    public IPEndPoint BindIpEndPoint { get; set; } = new(IPAddress.Any, 15124);

    public bool UseUpnp { get; set; } = false;
}