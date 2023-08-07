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
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Implementations.Packets.Types;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces.Packets;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Implementations.Packets;

internal abstract record Packet(PacketType PacketType = PacketType.Unknown, int Origin = -1) : IPacket
{
    private const string ProtocolVersion = "0.5";

    [ThreadStatic]
    private static StringBuilder? _stringBuilder;

    public static bool TryParse(ReadOnlySpan<char> rawData, [MaybeNullWhen(false)] out IPacket packet)
    {
        var currentPackageIndex = 0;
        var maxPackageIndex = 3;

        var packageType = PacketType.Unknown;
        var origin = -1;

        var dataIndexLength = 0;
        var dataIndexes = Array.Empty<int>();
        var currentDataIndex = 0;

        while (currentPackageIndex <= maxPackageIndex)
        {
            var nextDataLength = rawData.IndexOf('|');

            if (nextDataLength == -1)
            {
                packet = null;
                return false;
            }
            
            var data = rawData[..nextDataLength];

            switch (currentPackageIndex)
            {
                // Protocol Version
                case 0:
                {
                    if (!data.SequenceEqual(ProtocolVersion.AsSpan()))
                    {
                        packet = null;
                        return false;
                    }
                    break;
                }
                
                // Package Type
                case 1:
                {
                    if (!Enum.TryParse(data, out packageType))
                    {
                        packet = null;
                        return false;
                    }
                    break;
                }
                
                // Origin
                case 2:
                {
                    if (!int.TryParse(data, out origin))
                    {
                        packet = null;
                        return false;
                    }
                    break;
                }
                
                // Package Index Length
                case 3:
                {
                    if (!int.TryParse(data, out dataIndexLength))
                    {
                        packet = null;
                        return false;
                    }

                    if (dataIndexLength == 0) break;

                    maxPackageIndex += dataIndexLength;
                    dataIndexes = ArrayPool<int>.Shared.Rent(dataIndexLength);
                    break;
                }
                
                default:
                {
                    if (!int.TryParse(data, out var dataIndex))
                    {
                        packet = null;
                        return false;
                    }

                    dataIndexes[currentDataIndex++] = dataIndex;
                    break;
                }
            }
            
            currentPackageIndex++;
            rawData = rawData[(nextDataLength + 1)..];
        }
        
        if (dataIndexLength > 0)
        {
            ArrayPool<int>.Shared.Return(dataIndexes);
        }

        switch (packageType)
        {
            case PacketType.ServerDataRequest:
            {
                packet = new ServerDataRequestPacket();
                return true;
            }

            default:
            {
                packet = null;
                return false;
            }
        }
    }
    
    public string ToRawPacket()
    {
        _stringBuilder ??= new StringBuilder();
        _stringBuilder.Clear();

        _stringBuilder.Append(ProtocolVersion);
        _stringBuilder.Append('|');
        _stringBuilder.Append((int) PacketType);
        _stringBuilder.Append('|');
        _stringBuilder.Append(Origin);
        _stringBuilder.Append('|');

        var dataItems = GetDataItems();
        _stringBuilder.Append(dataItems.Length);
        
        var count = 0;

        foreach (var dataItem in dataItems)
        {
            _stringBuilder.Append('|');
            _stringBuilder.Append(count);

            count += dataItem.Length;
        }
        
        _stringBuilder.Append('|');

        foreach (var dataItem in dataItems)
        {
            _stringBuilder.Append(dataItem);
        }

        return _stringBuilder.ToString();
    }

    protected virtual string[] GetDataItems()
    {
        return Array.Empty<string>();
    }
}