using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheDialgaTeam.Pokemon3D.Server.Core.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Models;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Extensions;

public static class ServiceCollectionExtensions
{
    [RequiresDynamicCode("Binding strongly typed objects to configuration values may require generating dynamic code at runtime.")]
    [RequiresUnreferencedCode("NetworkOptions\'s and ServerOptions\'s dependent types may have their members trimmed. Ensure all required members are preserved.")]
    public static IServiceCollection AddPokemonServer(this IServiceCollection collection)
    {
        collection.AddPokemonServerOptions();
        collection.TryAddSingleton<PokemonServer>();
        
        return collection;
    }

    [RequiresDynamicCode("Binding strongly typed objects to configuration values may require generating dynamic code at runtime.")]
    [RequiresUnreferencedCode("NetworkOptions\'s and ServerOptions\'s dependent types may have their members trimmed. Ensure all required members are preserved.")]
    private static void AddPokemonServerOptions(this IServiceCollection collection)
    {
        collection.AddOptions<NetworkOptions>().BindConfiguration("Network");
        collection.AddOptions<ServerOptions>().BindConfiguration("Server");
        
        collection.TryAddSingleton<IPokemonServerOptions, PokemonServerOptions>();
    }
}