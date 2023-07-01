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
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Models;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options;

internal sealed class PokemonServerOptions : IPokemonServerOptions, IDisposable
{
    public NetworkOptions NetworkOptions { get; set; }

    public ServerOptions ServerOptions { get; set; }

    private readonly IDisposable? _networkOptionsDisposable;
    private readonly IDisposable? _serverOptionsDisposable;

    public PokemonServerOptions(IOptionsMonitor<NetworkOptions> networkOptions, IOptionsMonitor<ServerOptions> serverOptions)
    {
        NetworkOptions = networkOptions.CurrentValue;
        ServerOptions = serverOptions.CurrentValue;

        _networkOptionsDisposable = networkOptions.OnChange(options => NetworkOptions = options);
        _serverOptionsDisposable = serverOptions.OnChange(options => ServerOptions = options);
    }

    public void Dispose()
    {
        _networkOptionsDisposable?.Dispose();
        _serverOptionsDisposable?.Dispose();
    }
}