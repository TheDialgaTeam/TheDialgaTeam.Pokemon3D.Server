﻿// Pokemon 3D Server Client
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

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Converters;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Providers;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Validations;
using TheDialgaTeam.Pokemon3D.Server.Core.Player;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.World;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Extensions;

public static class ServiceCollectionExtensions
{
    [RequiresDynamicCode("Binding strongly typed objects to configuration values may require generating dynamic code at runtime.")]
    [RequiresUnreferencedCode("Dependent types may have their members trimmed. Ensure all required members are preserved.")]
    public static IServiceCollection AddPokemonServer(this IServiceCollection collection)
    {
        collection.TryAddSingleton<IPokemonServerListener, PokemonServerListener>();
        collection.TryAddSingleton<INatDeviceUtility, NatDeviceUtility>();
        collection.TryAddSingleton<IPokemonServerClientFactory, PokemonServerClientFactory>();
        
        TypeDescriptor.AddAttributes(typeof(IPEndPoint), new TypeConverterAttribute(typeof(IPEndPointConverter)));
        
        collection.AddOptions<ServerOptions>().BindConfiguration("Server");
        collection.AddOptions<NetworkOptions>().BindConfiguration("Server:Network");
        collection.AddOptions<WorldOptions>().BindConfiguration("Server:World");
        collection.AddOptions<ChatOptions>().BindConfiguration("Server:Chat");
        collection.AddOptions<PvPOptions>().BindConfiguration("Server:PvP");
        collection.AddOptions<TradeOptions>().BindConfiguration("Server:Trade");
        collection.AddOptions<LocalizationOptions>().BindConfiguration("Server:Localization");
        
        collection.TryAddSingleton<MicrosoftOptionsProvider>();
        collection.TryAddSingleton<IPokemonServerOptions>(provider => provider.GetRequiredService<MicrosoftOptionsProvider>());
        
        collection.TryAddSingleton<NetworkOptionsValidation>();
        collection.TryAddSingleton<IValidateOptions<NetworkOptions>>(provider => provider.GetRequiredService<NetworkOptionsValidation>());
        
        collection.TryAddSingleton<ServerOptionsValidation>();
        collection.TryAddSingleton<IValidateOptions<ServerOptions>>(provider => provider.GetRequiredService<ServerOptionsValidation>());
        
        collection.TryAddSingleton<IPlayerFactory, PlayerFactory>();
        
        collection.TryAddSingleton<ILocalWorld, LocalWorld>();
        collection.TryAddSingleton<ILocalWorldFactory, LocalWorldFactory>();

        return collection;
    }
}