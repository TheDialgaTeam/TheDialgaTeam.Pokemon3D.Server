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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Implementations.Packages;

public sealed record ServerInfoDataPackage(
    int PlayerCount,
    int MaxServerSize,
    string ServerName,
    string ServerDescription,
    string[] Players) : Package(PackageType.ServerInfoData)
{
    protected override string[] GetDataItems()
    {
        var result = new string[4 + Players.Length];

        result[0] = PlayerCount.ToString();
        result[1] = MaxServerSize.ToString();
        result[2] = ServerName;
        result[3] = ServerDescription;

        for (var i = 0; i < Players.Length; i++)
        {
            result[4 + i] = Players[i];
        }
        
        return result;
    }
}