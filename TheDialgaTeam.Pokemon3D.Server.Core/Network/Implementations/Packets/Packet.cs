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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Implementations.Packets;

public abstract record Packet(PacketType PacketType = PacketType.Unknown, int Origin = -1) : IPacket
{
    private const string ProtocolVersion = "0.5";

    [ThreadStatic]
    private static StringBuilder? _stringBuilder;
    
    public TaskCompletionSource TaskCompletionSource { get; } = new();

    /*
    public Package(string package)
    {
        var dataItemIndexes = Array.Empty<int>();

        try
        {
            var currentPackageIndex = 0;
            var maxPackageIndex = 3;
            var currentStringIndex = 0;
            var currentDataItemIndex = 0;

            while (currentPackageIndex <= maxPackageIndex)
            {
                var nextStringIndex = package.IndexOf('|', currentStringIndex);
                if (nextStringIndex == -1) break;

                var length = nextStringIndex - currentStringIndex;
                if (length == 0) break;

                switch (currentPackageIndex)
                {
                    case 0:
                        //ProtocolVersion = package.Substring(currentStringIndex, length);
                        break;

                    case 1:
                        PackageType = Enum.Parse<PackageType>(package.Substring(currentStringIndex, length));
                        break;

                    case 2:
                        Origin = int.Parse(package.Substring(currentStringIndex, length));
                        break;

                    case 3:
                        var packetLength = int.Parse(package.Substring(currentStringIndex, length));
                        maxPackageIndex = 3 + packetLength;
                        dataItemIndexes = ArrayPool<int>.Shared.Rent(packetLength);
                        break;

                    default:
                        dataItemIndexes[currentDataItemIndex++] = int.Parse(package.Substring(currentStringIndex, length));
                        break;
                }

                currentStringIndex = nextStringIndex + 1;
                currentPackageIndex++;
            }

            var dataItems = new string[dataItemIndexes.Length];

            for (var i = 0; i < dataItemIndexes.Length; i++)
            {
                dataItems[i] = i + 1 < dataItemIndexes.Length ? package.Substring(currentStringIndex + dataItemIndexes[i], dataItemIndexes[i + 1] - dataItemIndexes[i]) : package[(currentStringIndex + dataItemIndexes[i])..];
            }

            DataItems = dataItems;

            IsValid = true;
        }
        catch
        {
            IsValid = false;
        }
        finally
        {
            if (dataItemIndexes.Length > 0) ArrayPool<int>.Shared.Return(dataItemIndexes);
            TaskCompletionSource.SetResult();
        }
    }
    */

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
        
        if (dataIndexLength == 0)
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

    /*
    public bool IsFullPackageData()
    {
        return PackageType == PackageType.GameData && DataItems.Length == 15 && !string.IsNullOrWhiteSpace(DataItems[4]);
    }
    */

    protected virtual string[] GetDataItems()
    {
        return Array.Empty<string>();
    }
    
    public override string ToString()
    {
        _stringBuilder ??= new StringBuilder();
        
        _stringBuilder.Append(ProtocolVersion);
        _stringBuilder.Append('|');
        _stringBuilder.Append((int) PacketType);
        _stringBuilder.Append('|');
        _stringBuilder.Append(Origin);
        _stringBuilder.Append('|');

        var dataItems = GetDataItems();
        _stringBuilder.Append(dataItems.Length);

        var count = 0;

        foreach (var dataItem in GetDataItems())
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
}