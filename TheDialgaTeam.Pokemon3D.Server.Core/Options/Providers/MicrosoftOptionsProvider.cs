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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options.Providers;

internal sealed class MicrosoftOptionsProvider : IPokemonServerOptions, IDisposable
{
    public NetworkOptions NetworkOptions { get; private set; }

    public ServerOptions ServerOptions { get; private set; }

    public WorldOptions WorldOptions { get; private set; }

    public ChatOptions ChatOptions { get; private set; }

    public PvPOptions PvpOptions { get; private set; }

    public TradeOptions TradeOptions { get; private set; }

    private readonly IDisposable?[] _disposables;

    public MicrosoftOptionsProvider(
        IOptionsMonitor<NetworkOptions> networkOptions,
        IOptionsMonitor<ServerOptions> serverOptions,
        IOptionsMonitor<WorldOptions> worldOptions,
        IOptionsMonitor<ChatOptions> chatOptions,
        IOptionsMonitor<PvPOptions> pvpOptions,
        IOptionsMonitor<TradeOptions> tradeOptions)
    {
        NetworkOptions = networkOptions.CurrentValue;
        ServerOptions = serverOptions.CurrentValue;
        WorldOptions = worldOptions.CurrentValue;
        ChatOptions = chatOptions.CurrentValue;
        PvpOptions = pvpOptions.CurrentValue;
        TradeOptions = tradeOptions.CurrentValue;

        _disposables = new[]
        {
            networkOptions.OnChange(options => NetworkOptions = options),
            serverOptions.OnChange(options => ServerOptions = options),
            worldOptions.OnChange(options => WorldOptions = options),
            chatOptions.OnChange(options => ChatOptions = options),
            pvpOptions.OnChange(options => PvpOptions = options),
            tradeOptions.OnChange(options => TradeOptions = options)
        };
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable?.Dispose();
        }
    }
}