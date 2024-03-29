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

using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options.Providers;

internal sealed class MicrosoftGenericHostOptionsProvider : IPokemonServerOptions, IDisposable
{
    public ServerOptions ServerOptions { get; private set; }
    
    public NetworkOptions NetworkOptions { get; private set; }
    
    public DatabaseOptions DatabaseOptions { get; private set; }
    
    public SecurityOptions SecurityOptions { get; private set; }

    public WorldOptions WorldOptions { get; private set; }

    public ChatOptions ChatOptions { get; private set; }

    public PvPOptions PvpOptions { get; private set; }

    public TradeOptions TradeOptions { get; private set; }
    
    public LocalizationOptions LocalizationOptions { get; private set; }

    private readonly IDisposable?[] _disposables;

    public MicrosoftGenericHostOptionsProvider(
        IOptionsMonitor<ServerOptions> serverOptions,
        IOptionsMonitor<NetworkOptions> networkOptions,
        IOptionsMonitor<DatabaseOptions> dataBaseOptions,
        IOptionsMonitor<SecurityOptions> securityOptions,
        IOptionsMonitor<WorldOptions> worldOptions,
        IOptionsMonitor<ChatOptions> chatOptions,
        IOptionsMonitor<PvPOptions> pvpOptions,
        IOptionsMonitor<TradeOptions> tradeOptions,
        IOptionsMonitor<LocalizationOptions> localizationOptions)
    {
        ServerOptions = serverOptions.CurrentValue;
        NetworkOptions = networkOptions.CurrentValue;
        DatabaseOptions = dataBaseOptions.CurrentValue;
        SecurityOptions = securityOptions.CurrentValue;
        WorldOptions = worldOptions.CurrentValue;
        ChatOptions = chatOptions.CurrentValue;
        PvpOptions = pvpOptions.CurrentValue;
        TradeOptions = tradeOptions.CurrentValue;
        LocalizationOptions = localizationOptions.CurrentValue;

        _disposables =
        [
            serverOptions.OnChange(options => ServerOptions = options),
            networkOptions.OnChange(options => NetworkOptions = options),
            dataBaseOptions.OnChange(options => DatabaseOptions = options),
            securityOptions.OnChange(options => SecurityOptions = options),
            worldOptions.OnChange(options => WorldOptions = options),
            chatOptions.OnChange(options => ChatOptions = options),
            pvpOptions.OnChange(options => PvpOptions = options),
            tradeOptions.OnChange(options => TradeOptions = options),
            localizationOptions.OnChange(options => LocalizationOptions = options)
        ];
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable?.Dispose();
        }
    }
}