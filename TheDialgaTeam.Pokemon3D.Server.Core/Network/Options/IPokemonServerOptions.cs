using TheDialgaTeam.Pokemon3D.Server.Core.Network.Options.Models;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Options;

public interface IPokemonServerOptions
{
    NetworkOptions NetworkOptions { get; set; }
    
    ServerOptions ServerOptions { get; set; }
}