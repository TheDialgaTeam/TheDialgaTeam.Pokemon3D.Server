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
using TheDialgaTeam.Pokemon3D.Server.Core.World;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets.Types;

internal sealed record WorldDataPacket : Packet
{
    public Season Season
    {
        get => Enum.Parse<Season>(DataItems[0]);
        set => DataItems[0] = ((int) value).ToString(CultureInfo.InvariantCulture);
    }
    
    public Weather Weather
    {
        get => Enum.Parse<Weather>(DataItems[1]);
        set => DataItems[1] = ((int) value).ToString(CultureInfo.InvariantCulture);
    }

    public TimeOnly Time
    {
        get => TimeOnly.ParseExact(DataItems[2], "H,m,s");
        set => DataItems[2] = value.ToString("H,m,s");
    }
    
    public WorldDataPacket(string[] dataItems) : base(PacketType.WorldData)
    {
        DataItems = dataItems;
    }

    public WorldDataPacket(Season season, Weather weather, TimeOnly time) : base(PacketType.WorldData)
    {
        DataItems = new string[3];
        
        Season = season;
        Weather = weather;
        Time = time;
    }
}