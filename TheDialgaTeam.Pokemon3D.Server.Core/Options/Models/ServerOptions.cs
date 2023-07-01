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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options.Models;

internal sealed record ServerOptions
{
    public string ServerName { get; init; } = "Pokemon 3D Server";

    public string ServerDescription { get; init; } = string.Empty;

    public string WelcomeMessage { get; init; } = string.Empty;

    public string[] GameModes { get; init; } = { "Kolben" };

    public int MaxPlayers { get; init; } = 20;

    public bool OfflineMode { get; init; }

    public TimeSpan NoPingKickTime { get; init; } = TimeSpan.FromSeconds(10);

    public TimeSpan AwayFromKeyboardKickTime { get; init; } = TimeSpan.FromMinutes(5);

    public WorldOptions WorldOptions { get; init; } = new();

    public ChatOptions ChatOptions { get; init; } = new();

    public PvPOptions PvPOptions { get; init; } = new();

    public TradeOptions TradeOptions { get; init; } = new();
}

internal sealed record WorldOptions
{
    public int Season { get; init; } = -1;

    public int Weather { get; init; } = -1;

    public bool DoDayCycle { get; init; } = true;

    public int[] SeasonMonth { get; init; } = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

    public int[] WeatherSeason { get; init; } = { -1, -1, -1, -1 };
}

internal sealed record ChatOptions
{
    public bool AllowChat { get; init; } = true;

    public string[] ChatChannels { get; init; } = { "All" };
}

internal sealed record PvPOptions
{
    public bool AllowPvP { get; init; } = true;

    public bool AllowPvPValidation { get; init; } = true;
}

internal sealed record TradeOptions
{
    public bool AllowTrade { get; init; } = true;

    public bool AllowTradeValidation { get; init; } = true;
}