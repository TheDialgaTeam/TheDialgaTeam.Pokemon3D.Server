using TheDialgaTeam.Pokemon3D.Server.Core.Options.Models;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

public interface IPokemonServerOptions
{
    NetworkOptions NetworkOptions { get; set; }
    
    ServerOptions ServerOptions { get; set; }
}