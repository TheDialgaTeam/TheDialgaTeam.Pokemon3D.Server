using Microsoft.Extensions.Logging;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public class PokemonServerContext
{
    public PokemonServerOptions Options { get; }
    
    public ILogger Logger { get; }

    private readonly PokemonServer _server;

    public PokemonServerContext(PokemonServer server)
    {
        Options = server.Options;
        Logger = server.Logger;

        _server = server;
    }

    public async Task StartServerAsync()
    {
        await _server.StartAsync().ConfigureAwait(false);
    }

    public async Task StopServerAsync()
    {
        await _server.StopAsync().ConfigureAwait(false);
    }
}