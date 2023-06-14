using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Options;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public sealed class PokemonServerOptions : IDisposable
{
    public NetworkOptions NetworkOptions { get; set; }

    public ServerOptions ServerOptions { get; set; }

    private readonly IDisposable? _networkOptionsDisposable;
    private readonly IDisposable? _serverOptionsDisposable;

    public PokemonServerOptions(IOptionsMonitor<NetworkOptions> networkOptions, IOptionsMonitor<ServerOptions> serverOptions)
    {
        NetworkOptions = networkOptions.CurrentValue;
        ServerOptions = serverOptions.CurrentValue;
        
        _networkOptionsDisposable = networkOptions.OnChange(options => NetworkOptions = options);
        _serverOptionsDisposable = serverOptions.OnChange(options => ServerOptions = options);
    }

    public void Dispose()
    {
        _networkOptionsDisposable?.Dispose();
        _serverOptionsDisposable?.Dispose();
    }
}