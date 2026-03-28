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

using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Packets;

namespace TheDialgaTeam.Pokemon3D.Server.Test.Network.Packets;

public class RawPacketTest
{
    [Fact]
    public void SimpleRawPacketOutputTest()
    {
        var rawPacket = new RawPacket(RawPacket.ProtocolVersion, PacketType.Unknown, Origin.Server, []);

        Assert.Equal($"{RawPacket.ProtocolVersion}|{(int) PacketType.Unknown}|{Origin.Server.ToRawString()}|0|", rawPacket.ToRawPacketString());
    }

    [Fact]
    public void SimpleRawPacketWithDataItemsOutputTest()
    {
        var dataItems = new[] { "Test", "Test" };
        var rawPacket = new RawPacket(RawPacket.ProtocolVersion, PacketType.Unknown, Origin.Server, dataItems);

        Assert.Equal($"{RawPacket.ProtocolVersion}|{(int) PacketType.Unknown}|{Origin.Server.ToRawString()}|{dataItems.Length}|0|{dataItems[0].Length}|{dataItems[0]}{dataItems[1]}", rawPacket.ToRawPacketString());
    }
}