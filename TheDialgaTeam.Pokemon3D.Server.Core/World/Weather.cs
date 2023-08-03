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

namespace TheDialgaTeam.Pokemon3D.Server.Core.World;

public enum Weather
{
    Default = -1,
    Random = -2,
    WeatherSeason = -3,
    
    Clear = 0,
    Rain = 1,
    Snow = 2,
    Underwater = 3,
    Sunny = 4,
    Fog = 5,
    Thunderstorm = 6,
    Sandstorm = 7,
    Ash = 8,
    Blizzard = 9
}