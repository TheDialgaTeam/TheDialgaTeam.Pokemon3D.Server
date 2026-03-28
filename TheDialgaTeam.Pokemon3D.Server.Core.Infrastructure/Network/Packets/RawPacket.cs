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

using System.Text;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Packets;

public record RawPacket(string Version, PacketType PacketType, Origin Origin, string[] DataItems) : IRawPacket
{
    public const string ProtocolVersion = "0.5";

    [ThreadStatic]
    private static StringBuilder? t_stringBuilder;

    public string ToRawPacketString()
    {
        t_stringBuilder ??= new StringBuilder();
        t_stringBuilder.Clear();

        t_stringBuilder.Append(Version);
        t_stringBuilder.Append('|');
        t_stringBuilder.Append((int) PacketType);
        t_stringBuilder.Append('|');
        t_stringBuilder.Append(Origin);
        t_stringBuilder.Append('|');
        t_stringBuilder.Append(DataItems.Length);

        var count = 0;

        foreach (var dataItem in DataItems)
        {
            t_stringBuilder.Append('|');
            t_stringBuilder.Append(count);

            count += dataItem.Length;
        }

        t_stringBuilder.Append('|');

        foreach (var dataItem in DataItems)
        {
            t_stringBuilder.Append(dataItem);
        }

        return t_stringBuilder.ToString();
    }
}