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

using TheDialgaTeam.Pokemon3D.Server.Core.Domain.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;

namespace TheDialgaTeam.Pokemon3D.Server.Test.Network.Packets;

public sealed class RawPacketTest
{
    private static readonly string[] DataItems = ["Test", "Test"];
    private static readonly string[] ServerRequestPacketDataItems = ["r"];

    [Fact]
    public void SimpleRawPacketWithDataItemsParsingTest()
    {
        var rawPacket = new RawPacket(RawPacket.ProtocolVersion, PacketType.Unknown, Origin.Server, DataItems);
        Assert.Equal($"{RawPacket.ProtocolVersion}|{(int) PacketType.Unknown}|{(int) Origin.Server}|{rawPacket.DataItems.Length}|0|{DataItems[0].Length}|{DataItems[0]}{DataItems[1]}", rawPacket.ToRawPacketString());
    }

    [Fact]
    public void ServerRequestPacketTest()
    {
        var serverRequestPacket = new ServerDataRequestPacket("r");
        var expectedRawPacket = new RawPacket(RawPacket.ProtocolVersion, PacketType.ServerDataRequest, Origin.NewPlayer, ServerRequestPacketDataItems);
        
        Assert.Equal(expectedRawPacket.ToRawPacketString(), serverRequestPacket.ToClientRawPacket().ToRawPacketString());
        Assert.True(RawPacket.TryParse($"{RawPacket.ProtocolVersion}|{(int) PacketType.ServerDataRequest}|{(int) Origin.Server}|1|0|r", out var rawPacket));
        Assert.Equal(serverRequestPacket, new ServerDataRequestPacket(rawPacket));
    }
}