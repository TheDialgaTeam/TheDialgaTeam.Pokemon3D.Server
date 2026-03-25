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

using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options.Provider;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Listener.Factory;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Listener.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Upnp.Factory;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Upnp.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Options.Provider;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Options.Validator;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Extensions;

public static class HostBuilderExtensions
{
    public class PokemonServerBuilder(IServiceCollection collection)
    {
        private Type _networkListenerFactoryType = typeof(TcpNetworkListenerFactory);
        private Type _natDeviceServiceFactoryType = typeof(MonoNatDeviceServiceFactory);

        public PokemonServerBuilder UseTcpNetworkListener()
        {
            _networkListenerFactoryType = typeof(TcpNetworkListenerFactory);
            return this;
        }

        public PokemonServerBuilder UseMonoNatDeviceService()
        {
            _natDeviceServiceFactoryType = typeof(MonoNatDeviceServiceFactory);
            return this;
        }

        public PokemonServerBuilder UseEmptyNatDeviceService()
        {
            _natDeviceServiceFactoryType = typeof(EmptyNatDeviceServiceFactory);
            return this;
        }

        public PokemonServerBuilder UseDefaults()
        {
            UseTcpNetworkListener();
            return this;
        }

        public void Build()
        {
            collection.TryAddSingleton(typeof(INetworkListenerFactory), _networkListenerFactoryType);
            collection.TryAddSingleton(typeof(INatDeviceServiceFactory), _natDeviceServiceFactoryType);
        }
    }
    
    public static IHostBuilder ConfigurePokemonServerInfrastructure(this IHostBuilder hostBuilder, Action<PokemonServerBuilder> builderAction)
    {
        return hostBuilder.ConfigureServices(collection =>
        {
            var builder = new PokemonServerBuilder(collection);
            builderAction.Invoke(builder);
            builder.Build();
            
            collection.TryAddSingleton<NetworkClientContainer>();
            collection.TryAddSingleton<NetworkClientHandlerFactory>();
            collection.TryAddSingleton<IPokemonServerService, PokemonServerService>();

            collection.AddOptions<ServerOptions>().BindConfiguration("Server").ValidateOnStart();
            collection.TryAddSingleton<IValidateOptions<ServerOptions>, ServerOptionsValidator>();
            collection.TryAddSingleton<IServerOptionsProvider, MicrosoftExtensionsOptionsServerOptionsProvider>();
        }).ConfigureAppConfiguration(static builder => { builder.AddJsonFile(builder.GetFileProvider(), "server.json", true, true); });
    }
}