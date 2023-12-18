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
using TheDialgaTeam.Pokemon3D.Server.Core.World;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;

public sealed record WorldDataPacket(Season Season, Weather Weather, TimeOnly Time) : IPacket
{
    public WorldDataPacket(IRawPacket rawPacket) : this(
        Enum.Parse<Season>(rawPacket.DataItems[0]), 
        Enum.Parse<Weather>(rawPacket.DataItems[1]), 
        TimeOnly.ParseExact(rawPacket.DataItems[2], "H,m,s"))
    {
    }

    public IRawPacket ToServerRawPacket()
    {
        return new RawPacket(RawPacket.ProtocolVersion, PacketType.WorldData, Origin.Server, new[]
        {
            ((int) Season).ToString(CultureInfo.InvariantCulture),
            ((int) Weather).ToString(CultureInfo.InvariantCulture),
            Time.ToString("H,m,s")
        });
    }

    public IRawPacket ToClientRawPacket()
    {
        throw new NotSupportedException();
    }
}