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

using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Models;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Queries;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options;

internal sealed class PokemonServerOptions : 
    IRequestHandler<GetNetworkOptions, NetworkOptions>,
    IRequestHandler<GetServerOptions, ServerOptions>,
    IDisposable
{
    public NetworkOptions NetworkOptions { get; private set; }

    public ServerOptions ServerOptions { get; private set; }

    private readonly IDisposable? _networkOptionsDisposable;
    private readonly IDisposable? _serverOptionsDisposable;

    public PokemonServerOptions(IOptionsMonitor<NetworkOptions> networkOptions, IOptionsMonitor<ServerOptions> serverOptions)
    {
        NetworkOptions = networkOptions.CurrentValue;
        ServerOptions = serverOptions.CurrentValue;

        _networkOptionsDisposable = networkOptions.OnChange(options => NetworkOptions = options);
        _serverOptionsDisposable = serverOptions.OnChange(options => ServerOptions = options);
    }

    public Task<NetworkOptions> HandleAsync(GetNetworkOptions request, CancellationToken cancellationToken)
    {
        return Task.FromResult(NetworkOptions);
    }
    
    public Task<ServerOptions> HandleAsync(GetServerOptions request, CancellationToken cancellationToken)
    {
        return Task.FromResult(ServerOptions);
    }

    public void Dispose()
    {
        _networkOptionsDisposable?.Dispose();
        _serverOptionsDisposable?.Dispose();
    }
}