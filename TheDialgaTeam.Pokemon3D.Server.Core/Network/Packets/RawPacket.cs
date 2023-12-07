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

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces.Packets;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;

public sealed record RawPacket(PacketType PacketType, Origin Origin, string[] DataItems) : IRawPacket
{
    private const string ProtocolVersion = "0.5";

    [ThreadStatic]
    private static StringBuilder? t_stringBuilder;

    public static bool TryParse(ReadOnlySpan<char> rawData, [MaybeNullWhen(false)] out IRawPacket rawPacket)
    {
        if (rawData.IsEmpty)
        {
            rawPacket = null;
            return false;
        }
        
        var currentPacketIndex = 0;
        var maxPacketIndex = 3;

        var packetType = PacketType.Unknown;
        var origin = Origin.Server;

        var dataItemIndexLength = 0;
        var dataItemIndexes = Array.Empty<int>();
        var currentDataItemIndex = 0;

        var dataItemIndex = 0;
        var dataItems = Array.Empty<string>();

        try
        {
            while (currentPacketIndex <= maxPacketIndex)
            {
                var nextDataLength = rawData.IndexOf('|');

                if (nextDataLength == -1)
                {
                    rawPacket = null;
                    return false;
                }

                var data = rawData[..nextDataLength];

                switch (currentPacketIndex)
                {
                    // Protocol Version
                    case 0:
                    {
                        if (!data.SequenceEqual(ProtocolVersion.AsSpan()))
                        {
                            rawPacket = null;
                            return false;
                        }

                        break;
                    }

                    // Package Type
                    case 1:
                    {
                        if (!Enum.TryParse(data, out packetType))
                        {
                            rawPacket = null;
                            return false;
                        }

                        break;
                    }

                    // Origin
                    case 2:
                    {
                        if (!int.TryParse(data, out var originId))
                        {
                            rawPacket = null;
                            return false;
                        }

                        origin = originId;
                        break;
                    }

                    // Package Index Length
                    case 3:
                    {
                        if (!int.TryParse(data, out dataItemIndexLength))
                        {
                            rawPacket = null;
                            return false;
                        }

                        if (dataItemIndexLength == 0) break;

                        maxPacketIndex += dataItemIndexLength;
                        dataItemIndexes = ArrayPool<int>.Shared.Rent(dataItemIndexLength);
                        dataItems = new string[dataItemIndexLength];
                        break;
                    }

                    default:
                    {
                        if (!int.TryParse(data, out var dataIndex))
                        {
                            rawPacket = null;
                            return false;
                        }

                        dataItemIndexes[currentDataItemIndex++] = dataIndex;
                        break;
                    }
                }

                currentPacketIndex++;
                rawData = rawData[(nextDataLength + 1)..];
            }

            if (dataItemIndexLength > 0)
            {
                for (; dataItemIndex < dataItemIndexLength - 1; dataItemIndex++)
                {
                    dataItems[dataItemIndex] = rawData[dataItemIndexes[dataItemIndex]..dataItemIndexes[dataItemIndex + 1]].ToString();
                }

                dataItems[dataItemIndex] = rawData[dataItemIndexes[dataItemIndex]..].ToString();
            }

            rawPacket = new RawPacket(packetType, origin, dataItems);
            return true;
        }
        finally
        {
            if (dataItemIndexLength > 0)
            {
                ArrayPool<int>.Shared.Return(dataItemIndexes);
            }
        }
    }

    public string ToRawPacketString()
    {
        t_stringBuilder ??= new StringBuilder();
        t_stringBuilder.Clear();

        t_stringBuilder.Append(ProtocolVersion);
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