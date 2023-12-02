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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Database;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Providers;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Validations;
using TheDialgaTeam.Pokemon3D.Server.Core.Player;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.World;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Extensions;

public static class HostBuilderExtensions
{
    [RequiresDynamicCode("Binding strongly typed objects to configuration values may require generating dynamic code at runtime.")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Dependent types have been preserved.")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SqliteOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(LocalizedString))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PlayerNameDisplayFormat))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ConsoleMessageFormat))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(GameMessageFormat))]
    public static IHostBuilder ConfigurePokemonServer(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(collection =>
        {
            collection.TryAddSingleton<INatDevicePortMapper, NatDevicePortMapper>();
            collection.TryAddSingleton<IPokemonServerClientFactory, PokemonServerClientFactory>();
            collection.TryAddSingleton<IPlayerFactory, PlayerFactory>();
            collection.TryAddSingleton<ILocalWorldFactory, LocalWorldFactory>();
            collection.TryAddSingleton<IStringLocalizer, OptionsStringLocalizer>();
            
            collection.AddOptions<ServerOptions>().BindConfiguration("Server");
            collection.TryAddSingleton<ServerOptionsValidation>();
            collection.TryAddSingleton<IValidateOptions<ServerOptions>>(static provider => provider.GetRequiredService<ServerOptionsValidation>());

            collection.AddOptions<NetworkOptions>().BindConfiguration("Server:Network");
            collection.TryAddSingleton<NetworkOptionsValidation>();
            collection.TryAddSingleton<IValidateOptions<NetworkOptions>>(static provider => provider.GetRequiredService<NetworkOptionsValidation>());

            collection.AddOptions<DatabaseOptions>().BindConfiguration("Server:Database");
            collection.TryAddSingleton<DatabaseOptionsValidation>();
            collection.TryAddSingleton<IValidateOptions<DatabaseOptions>>(static provider => provider.GetRequiredService<DatabaseOptionsValidation>());

            collection.AddOptions<WorldOptions>().BindConfiguration("Server:World");
            collection.AddOptions<ChatOptions>().BindConfiguration("Server:Chat");
            collection.AddOptions<PvPOptions>().BindConfiguration("Server:PvP");
            collection.AddOptions<TradeOptions>().BindConfiguration("Server:Trade");
            collection.AddOptions<LocalizationOptions>().BindConfiguration("Localization");

            collection.TryAddSingleton<MicrosoftGenericHostOptionsProvider>();
            collection.TryAddSingleton<IPokemonServerOptions>(static provider => provider.GetRequiredService<MicrosoftGenericHostOptionsProvider>());

            collection.AddDbContextFactory<DatabaseContext>();
        }).ConfigureAppConfiguration(builder =>
        {
            builder.AddJsonFile("localization.json", true, true);
        });
    }
}