// Pokemon 3D Server Client
// Copyright (C) 2023 Yong Jian Ming
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Attributes;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Models;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options.Extensions;

public static partial class ServiceCollectionExtensions
{
    [RequiresDynamicCode("Binding strongly typed objects to configuration values may require generating dynamic code at runtime.")]
    [RequiresUnreferencedCode("Dependent types may have their members trimmed. Ensure all required members are preserved.")]
    public static IServiceCollection AddPokemonServerOptions(this IServiceCollection collection)
    {
        collection.AddOptions<NetworkOptions>().BindConfiguration("Network");
        collection.AddOptions<ServerOptions>().BindConfiguration("Server");
        collection.AddOptions<WorldOptions>().BindConfiguration("Server:World");
        collection.AddOptions<ChatOptions>().BindConfiguration("Server:Chat");
        collection.AddOptions<PvPOptions>().BindConfiguration("Server:PvP");
        collection.AddOptions<TradeOptions>().BindConfiguration("Server:Trade");
        
        collection.TryAddSingleton<PokemonServerOptions>();
        AddQueryHandler(collection);
        
        return collection;
    }

    [QueryHandler(typeof(PokemonServerOptions))]
    private static partial void AddQueryHandler(IServiceCollection collection);
}