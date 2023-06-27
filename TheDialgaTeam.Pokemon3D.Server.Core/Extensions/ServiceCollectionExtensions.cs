using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheDialgaTeam.Pokemon3D.Server.Core.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Options.Models;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPokemonServer(this IServiceCollection collection)
    {
        collection.AddPokemonServerOptions();
        collection.TryAddSingleton<PokemonServer>();
        
        return collection;
    }

    private static void AddPokemonServerOptions(this IServiceCollection collection)
    {
        collection.AddOptions<NetworkOptions>().BindConfiguration("Network");
        collection.AddOptions<ServerOptions>().BindConfiguration("Server");
        
        collection.TryAddSingleton<IPokemonServerOptions, PokemonServerOptions>();
    }
}