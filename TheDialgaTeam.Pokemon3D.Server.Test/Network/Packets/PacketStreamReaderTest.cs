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

using System.Diagnostics.CodeAnalysis;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Packets;

namespace TheDialgaTeam.Pokemon3D.Server.Test.Network.Packets;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public class PacketStreamReaderTest
{
    public static TheoryData<string> BadRawPacketParsingTestData =
    [
        "",
        "|",
        "SOME_GARBAGE|",
        "0.5|SOME_GARBAGE|",
        "0.5|-99|",
        "0.5|-1|SOME_GARBAGE|",
        "0.5|-1|-1|SOME_GARBAGE|",
        "0.5|-1|-1|0|SOME_GARBAGE",
        "0.5|-1|-1|1|SOME_GARBAGE|",
        "0.5|-1|-1|1|0|" + string.Concat(Enumerable.Repeat("A", 4*1024*1024)),
        "0.5|-1|-1|2|0|SOME_GARBAGE|",
        "0.5|-1|-1|2|0|4|Tes"
    ];
    
    public static TheoryData<string, string, PacketType, Origin, string[]> GoodRawPacketParsingTestData =
    [
        new TheoryDataRow<string, string, PacketType, Origin, string[]>("0.5|-1|-1|0|", "0.5", PacketType.Unknown, Origin.Server, []),
        new TheoryDataRow<string, string, PacketType, Origin, string[]>("0.5|-1|-1|1|0|Test", "0.5", PacketType.Unknown, Origin.Server, ["Test"]),
        new TheoryDataRow<string, string, PacketType, Origin, string[]>("0.5|-1|-1|2|0|4|TestTest", "0.5", PacketType.Unknown, Origin.Server, ["Test", "Test"]),
        new TheoryDataRow<string, string, PacketType, Origin, string[]>("0.5|99|0|1|0|r", "0.5", PacketType.ServerDataRequest, Origin.NewPlayer, ["r"])
    ];
    
    [Theory]
    [MemberData(nameof(BadRawPacketParsingTestData))]
    public async Task BadRawPacketParsingTest(string rawData)
    {
        using var memoryStream = new MemoryStream();
        await using var writer = new StreamWriter(memoryStream);
        await writer.WriteLineAsync(rawData);
        await writer.FlushAsync(TestContext.Current.CancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);

        var reader = new PacketStreamReader(memoryStream, (int) memoryStream.Length);
        
        Assert.Null(await reader.ReadPacketAsync(TestContext.Current.CancellationToken));
    }
    
    [Theory]
    [MemberData(nameof(GoodRawPacketParsingTestData))]
    public async Task GoodRawPacketParsingTest(string rawData, string version, PacketType packetType, Origin origin, string[] dataItems)
    {
        using var memoryStream = new MemoryStream();
        await using var writer = new StreamWriter(memoryStream);
        await writer.WriteLineAsync(rawData);
        await writer.FlushAsync(TestContext.Current.CancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);

        var reader = new PacketStreamReader(memoryStream, (int) memoryStream.Length);
        var packet = await reader.ReadPacketAsync(TestContext.Current.CancellationToken);
        
        Assert.NotNull(packet);
        Assert.Equal(version, packet.Version);
        Assert.Equal(packetType, packet.PacketType);
        Assert.Equal(origin, packet.Origin);
        Assert.True(packet.DataItems.Length == dataItems.Length);
        Assert.True(packet.DataItems.SequenceEqual(dataItems));
    }
}