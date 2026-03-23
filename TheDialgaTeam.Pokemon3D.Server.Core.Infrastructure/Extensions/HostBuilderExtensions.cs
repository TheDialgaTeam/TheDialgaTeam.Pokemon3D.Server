// Pokemon 3D Server Client
// Copyright (C) 2026 Yong Jian Ming
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
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options.Provider;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.World;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Listener;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Options.Provider;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Options.Validator;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.World;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigurePokemonServerInfrastructure(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(static collection =>
        {
            collection.TryAddSingleton<INetworkListener, TcpNetworkListener>();
            collection.TryAddSingleton<IPokemonServerService, PokemonServerService>();
            
            collection.AddOptions<ServerOptions>().BindConfiguration("Server").ValidateOnStart();
            collection.TryAddSingleton<IValidateOptions<ServerOptions>, ServerOptionsValidator>();
            collection.TryAddSingleton<IServerOptionsProvider, MicrosoftExtensionsOptionsServerOptionsProvider>();
            
            collection.TryAddSingleton<ILocalWorldFactory, LocalWorldFactory>();
        }).ConfigureAppConfiguration(static builder =>
        {
            builder.AddJsonFile(builder.GetFileProvider(), "server.json", true, true);
        });
    }
}