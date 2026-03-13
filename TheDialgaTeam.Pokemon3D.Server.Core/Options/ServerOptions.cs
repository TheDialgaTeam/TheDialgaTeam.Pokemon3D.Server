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
using TheDialgaTeam.Pokemon3D.Server.Core.World;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options;

public sealed class ServerOptions
{
    #region General

    public string ServerName { get; set; } = "Pokemon 3D Server";
    public string ServerDescription { get; set; } = string.Empty;

    public string[] WelcomeMessage { get; set; } = [];

    public bool AllowAnyGameModes { get; set; }
    public string[] WhitelistedGameModes { get; set; } = [];
    public string[] BlacklistedGameModes { get; set; } = [];

    public int MaxPlayers { get; set; } = 20;
    public bool AllowOfflinePlayer { get; set; }

    public int AwayFromKeyboardKickTime { get; set; } = 60 * 5;
    public int NoPingKickTime { get; set; } = 30;

    #endregion

    #region Network

    public string BindingInformation { get; set; } = new IPEndPoint(IPAddress.Any, 15124).ToString();

    public bool UseUpnp { get; set; }
    public int UpnpDiscoveryTime { get; set; } = 5;

    #endregion

    #region World

    public bool DoDayCycle { get; set; } = true;
    public TimeSpan TimeOffset { get; set; } = DateTimeOffset.Now.Offset;

    public Season Season { get; set; } = Season.Default;
    public Weather Weather { get; set; } = Weather.Default;

    public int[] SeasonMonth { get; set; } = [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1];
    public int[] WeatherSeason { get; set; } = [-1, -1, -1, -1];

    #endregion

    #region Chat

    public bool AllowChat { get; set; } = true;
    public string[] ChatChannels { get; set; } = ["All"];

    #endregion

    #region Trade

    public bool AllowTrade { get; set; } = true;
    public bool AllowTradeValidation { get; set; } = true;

    #endregion

    #region PvP

    public bool AllowPvP { get; set; } = true;
    public bool AllowPvPValidation { get; set; } = true;

    #endregion
}