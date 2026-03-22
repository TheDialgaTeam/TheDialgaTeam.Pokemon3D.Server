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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Network.Listener;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Network.Upnp;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options.Provider;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Listener;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Upnp;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Options.Provider;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Options.Validator;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigurePokemonServer(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((_, collection) =>
        {
            collection.TryAddSingleton<INetworkListenerFactory, TcpNetworkListenerFactory>();
            collection.TryAddSingleton<INatDeviceUtility, NatDeviceUtility>();
            
            collection.AddOptions<ServerOptions>().BindConfiguration("Server").ValidateOnStart();
            collection.AddServerOptionsValidator();
            collection.TryAddSingleton<IServerOptionsProvider, MicrosoftExtensionsOptionsServerOptionsProvider>();
        });
    }
}