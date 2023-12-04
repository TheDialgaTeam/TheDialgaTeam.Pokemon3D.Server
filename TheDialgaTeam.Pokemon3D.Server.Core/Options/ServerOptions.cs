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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options;

public sealed record ServerOptions
{
    public string ServerName { get; set; } = "Pokemon 3D Server";

    public string ServerDescription { get; set; } = string.Empty;

    public string[] WelcomeMessage { get; set; } = Array.Empty<string>();

    public bool AllowAnyGameModes { get; set; }

    public string[] WhitelistedGameModes { get; set; } = Array.Empty<string>();

    public string[] BlacklistedGameModes { get; set; } = Array.Empty<string>();

    public int MaxPlayers { get; set; } = 20;

    public bool OfflineMode { get; set; }

    public int AwayFromKeyboardKickTime { get; set; } = 60 * 5;
    
    public int NoPingKickTime { get; set; } = 30;
}