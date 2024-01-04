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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Database;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization;
using TheDialgaTeam.Pokemon3D.Server.Core.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Providers;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Validations;
using TheDialgaTeam.Pokemon3D.Server.Core.Player;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Security.PasswordHashing;
using TheDialgaTeam.Pokemon3D.Server.Core.World;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigurePokemonServer(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(collection =>
        {
            collection.TryAddSingleton<HttpClient>(_ => new HttpClient());
            
            collection.TryAddSingleton<IPokemonServerClientFactory, PokemonServerClientFactory>();
            collection.TryAddSingleton<IPlayerFactory, PlayerFactory>();
            collection.TryAddSingleton<ILocalWorldFactory, LocalWorldFactory>();
            
            collection.AddOptionsWithValidateOnStart<ServerOptions>().BindConfiguration("Server");
            collection.TryAddSingleton<IValidateOptions<ServerOptions>, ServerOptionsValidation>();

            collection.AddOptionsWithValidateOnStart<NetworkOptions>().BindConfiguration("Server:Network");
            collection.TryAddSingleton<IValidateOptions<NetworkOptions>, NetworkOptionsValidation>();

            collection.AddOptionsWithValidateOnStart<DatabaseOptions>().BindConfiguration("Server:Database");
            collection.TryAddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidation>();

            collection.AddOptionsWithValidateOnStart<SecurityOptions>().BindConfiguration("Server:Security");
            collection.TryAddSingleton<IValidateOptions<SecurityOptions>, SecurityOptionsValidation>();
            
            collection.AddOptions<WorldOptions>().BindConfiguration("Server:World");
            collection.AddOptions<ChatOptions>().BindConfiguration("Server:Chat");
            collection.AddOptions<PvPOptions>().BindConfiguration("Server:PvP");
            collection.AddOptions<TradeOptions>().BindConfiguration("Server:Trade");
            
            collection.AddOptions<LocalizationOptions>().BindConfiguration("Localization");
            collection.TryAddSingleton<IStringLocalizer, JsonOptionsStringLocalizer>();
            
            collection.TryAddSingleton<IPokemonServerOptions, MicrosoftGenericHostOptionsProvider>();

            collection.AddDbContextFactory<DatabaseContext>();
            
            collection.TryAddSingleton<IPasswordHashingFactory, PasswordHashingFactory>();
        }).ConfigureAppConfiguration(builder =>
        {
            builder.AddJsonFile("localization.json", true, true);
        });
    }
}