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
using TheDialgaTeam.Pokemon3D.Server.Core.Domain.World;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Application.Options;

public class ServerOptions
{
    public string BindingInformation { get; set; } = new IPEndPoint(IPAddress.Any, 15124).ToString();

    public bool UseUpnp { get; set; }
    public int UpnpDiscoveryTime { get; set; } = 5;

    public string ServerName { get; set; } = "Pokemon 3D Server";
    public string ServerDescription { get; set; } = string.Empty;

    public string[] WelcomeMessage { get; set; } = [];

    public bool AllowAnyGameModes { get; set; }
    public string[] WhitelistedGameModes { get; set; } = ["Kolben"];
    public string[] BlacklistedGameModes { get; set; } = [];

    public int MaxPlayers { get; set; } = 20;
    public bool AllowOfflinePlayer { get; set; }

    public int AwayFromKeyboardKickTime { get; set; } = 60 * 5;
    public int NoPingKickTime { get; set; } = 30;

    public bool DoDayCycle { get; set; } = true;
    public TimeSpan TimeOffset { get; set; } = DateTimeOffset.Now.Offset;

    public Season Season { get; set; } = Season.Default;
    public Weather Weather { get; set; } = Weather.Default;

    public SeasonMonthValue[][] SeasonMonth { get; set; } =
    [
        [new SeasonMonthValue { Season = Season.Default, Chance = 1 }],
        [new SeasonMonthValue { Season = Season.Default, Chance = 1 }],
        [new SeasonMonthValue { Season = Season.Default, Chance = 1 }],
        [new SeasonMonthValue { Season = Season.Default, Chance = 1 }],
        [new SeasonMonthValue { Season = Season.Default, Chance = 1 }],
        [new SeasonMonthValue { Season = Season.Default, Chance = 1 }],
        [new SeasonMonthValue { Season = Season.Default, Chance = 1 }],
        [new SeasonMonthValue { Season = Season.Default, Chance = 1 }],
        [new SeasonMonthValue { Season = Season.Default, Chance = 1 }],
        [new SeasonMonthValue { Season = Season.Default, Chance = 1 }],
        [new SeasonMonthValue { Season = Season.Default, Chance = 1 }],
        [new SeasonMonthValue { Season = Season.Default, Chance = 1 }]
    ];

    public WeatherSeasonValue[][] WeatherSeason { get; set; } =
    [
        [new WeatherSeasonValue { Weather = Weather.Default, Chance = 1 }],
        [new WeatherSeasonValue { Weather = Weather.Default, Chance = 1 }],
        [new WeatherSeasonValue { Weather = Weather.Default, Chance = 1 }],
        [new WeatherSeasonValue { Weather = Weather.Default, Chance = 1 }]
    ];

    public bool AllowChat { get; set; } = true;

    public bool AllowTrade { get; set; } = true;
    public bool AllowTradeValidation { get; set; } = true;

    public bool AllowPvP { get; set; } = true;
    public bool AllowPvPValidation { get; set; } = true;

    public Dictionary<string, GameModeOverrideOptions> GameModeOverrides { get; set; } = new();
}

public class GameModeOverrideOptions
{
    public string[]? WelcomeMessage { get; set; }

    public bool? DoDayCycle { get; set; }
    public TimeSpan? TimeOffset { get; set; }

    public Season? Season { get; set; }
    public Weather? Weather { get; set; }

    public SeasonMonthValue[][]? SeasonMonth { get; set; }
    public WeatherSeasonValue[][]? WeatherSeason { get; set; }

    public bool? AllowChat { get; set; }

    public bool? AllowTrade { get; set; }
    public bool? AllowTradeValidation { get; set; }

    public bool? AllowPvP { get; set; }
    public bool? AllowPvPValidation { get; set; }

    public void SetDefaults(ServerOptions options)
    {
        WelcomeMessage ??= options.WelcomeMessage;

        DoDayCycle ??= options.DoDayCycle;
        TimeOffset ??= options.TimeOffset;

        Season ??= options.Season;
        Weather ??= options.Weather;

        SeasonMonth ??= options.SeasonMonth;
        WeatherSeason ??= options.WeatherSeason;

        AllowChat ??= options.AllowChat;

        AllowTrade ??= options.AllowTrade;
        AllowTradeValidation ??= options.AllowTradeValidation;

        AllowPvP ??= options.AllowPvP;
        AllowPvPValidation ??= options.AllowPvPValidation;
    }
}

public class SeasonMonthValue
{
    public Season Season { get; set; } = Season.Default;
    public int Chance { get; set; } = 1;
}

public class WeatherSeasonValue
{
    public Weather Weather { get; set; } = Weather.Default;
    public int Chance { get; set; } = 1;
}