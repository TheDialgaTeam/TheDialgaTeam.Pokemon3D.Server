using System.Net;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Options.Models;

public sealed class NetworkOptions
{
    public IPEndPoint BindIpEndPoint { get; set; } = new(IPAddress.Any, 15124);
    
    public string PublicIpAddress { get; set; } = string.Empty;

    public bool UseUpnp { get; set; } = false;
}