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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Localization;

public sealed class GameMessageFormat
{
    public string ServerChatMessage { get; init; } = "<{0}>: {1}";
    
    public string ServerIsFull { get; init; } = "This server is currently full of players.";
    public string ServerOnlyAllowGameJoltProfile { get; init; } = "This server do not allow offline profile.";
    public string ServerWhitelistedGameModes { get; init; } = "This server require you to play the following GameMode(s): {0}";
    public string ServerBlacklistedGameModes { get; init; } = "This server do not allow this GameMode to join.";
}