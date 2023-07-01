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
using System.Text;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Packages;

public sealed class Package
{
    public string ProtocolVersion { get; } = "0.5";

    public PackageType PackageType { get; } = PackageType.Unknown;

    public int Origin { get; } = -1;

    public string[] DataItems { get; } = Array.Empty<string>();

    public bool IsValid { get; }

    public TaskCompletionSource TaskCompletionSource { get; } = new();

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
                        ProtocolVersion = package.Substring(currentStringIndex, length);
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

    public Package(PackageType packageType, string[] dataItems, int origin = -1)
    {
        PackageType = packageType;
        Origin = origin;
        DataItems = dataItems;
        IsValid = true;
    }

    public Package(PackageType packageType, string dataItem, int origin = -1)
    {
        PackageType = packageType;
        Origin = origin;
        DataItems = new[] { dataItem };
        IsValid = true;
    }

    public bool IsFullPackageData()
    {
        return PackageType == PackageType.GameData && DataItems.Length == 15 && !string.IsNullOrWhiteSpace(DataItems[4]);
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(ProtocolVersion);
        stringBuilder.Append('|');
        stringBuilder.Append((int) PackageType);
        stringBuilder.Append('|');
        stringBuilder.Append(Origin);
        stringBuilder.Append('|');
        stringBuilder.Append(DataItems.Length);

        var count = 0;

        foreach (var dataItem in DataItems)
        {
            stringBuilder.Append('|');
            stringBuilder.Append(count);

            count += dataItem.Length;
        }

        stringBuilder.Append('|');

        foreach (var dataItem in DataItems)
        {
            stringBuilder.Append(dataItem);
        }

        return stringBuilder.ToString();
    }
}