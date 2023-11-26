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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets.Types;

internal sealed record ServerInfoDataPacket : Packet
{
    public int PlayerCount
    {
        get => int.Parse(DataItems[0], CultureInfo.InvariantCulture);
        set => DataItems[0] = value.ToString(CultureInfo.InvariantCulture);
    }

    public int MaxServerSize
    {
        get => int.Parse(DataItems[1], CultureInfo.InvariantCulture);
        set => DataItems[1] = value.ToString(CultureInfo.InvariantCulture);
    }

    public string ServerName
    {
        get => DataItems[2];
        set => DataItems[2] = value;
    }

    public string ServerDescription
    {
        get => DataItems[3];
        set => DataItems[3] = value;
    }

    public Span<string> Players => DataItems.AsSpan(4);

    public ServerInfoDataPacket(string[] dataItems) : base(PacketType.ServerInfoData)
    {
        DataItems = dataItems;
    }
    
    public ServerInfoDataPacket(
        int playerCount, 
        int maxServerSize, 
        string serverName, 
        string serverDescription, 
        ReadOnlySpan<string> players) : base(PacketType.ServerInfoData)
    {
        DataItems = new string[4 + Math.Min(playerCount, 21)];

        PlayerCount = playerCount;
        MaxServerSize = maxServerSize;
        ServerName = serverName;
        ServerDescription = serverDescription;

        players[..Math.Min(playerCount, 20)].CopyTo(Players);
        
        if (playerCount > 20)
        {
            DataItems[^1] = $"({playerCount - 20} more...)";
        }
    }
}