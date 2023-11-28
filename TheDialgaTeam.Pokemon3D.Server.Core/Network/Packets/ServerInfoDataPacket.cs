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

using System.Globalization;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces.Packets;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;

public readonly record struct ServerInfoDataPacket(
    int PlayerCount, 
    int MaxServerSize, 
    string ServerName, 
    string ServerDescription, 
    string[] PlayerNames) : IPacket
{
    public ServerInfoDataPacket(RawPacket rawPacket) : this(
        int.Parse(rawPacket.DataItems[0], CultureInfo.InvariantCulture),
        int.Parse(rawPacket.DataItems[1], CultureInfo.InvariantCulture),
        rawPacket.DataItems[2],
        rawPacket.DataItems[3],
        Array.Empty<string>())
    {
        PlayerNames = new string[PlayerCount];
        rawPacket.DataItems.AsSpan(4).CopyTo(PlayerNames);
    }

    public RawPacket ToRawPacket()
    {
        var dataItems = new string[4 + Math.Min(PlayerCount, 21)];
        dataItems[0] = PlayerCount.ToString(CultureInfo.InvariantCulture);
        dataItems[1] = MaxServerSize.ToString(CultureInfo.InvariantCulture);
        dataItems[2] = ServerName;
        dataItems[3] = ServerDescription;
        
        PlayerNames[..Math.Min(PlayerCount, 20)].AsSpan().CopyTo(dataItems.AsSpan()[4..]);
        
        if (PlayerCount > 20)
        {
            dataItems[^1] = $"({PlayerCount - 20} more...)";
        }

        return new RawPacket(PacketType.ServerInfoData, -1, dataItems);
    }
}