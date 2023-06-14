using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheDialgaTeam.Pokemon3D.Server.Core.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Nat;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Options;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Extensions;

public static class PokemonServerExtensions
{
    public static IServiceCollection AddPokemonServer(this IServiceCollection collection)
    {
        collection.AddOptions<NetworkOptions>().BindConfiguration("Network");
        collection.AddOptions<ServerOptions>().BindConfiguration("Server");

        collection.TryAddSingleton<HttpClient>(_ => new HttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) }));
        
        collection.TryAddSingleton<NatDeviceListener>();
        
        collection.TryAddSingleton<PokemonServerOptions>();
        collection.TryAddSingleton<PokemonServer>();
        
        return collection;
    }
}