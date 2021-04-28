using System;
using System.Collections.Generic;
using System.Text;

namespace TheDialgaTeam.Pokemon3D.Server.Packages
{
    internal class Package
    {
        public string ProtocolVersion { get; } = string.Empty;

        public PackageType PackageType { get; } = PackageType.Unknown;

        public int Origin { get; } = -1;

        public IReadOnlyList<string> DataItems { get; } = new List<string>();

        public bool IsValid { get; }

        public Package(string package)
        {
            var currentPackageIndex = 0;
            var maxPackageIndex = 3;
            var currentStringIndex = 0;
            var dataItemIndexes = new List<int>();
            var dataItems = new List<string>();

            try
            {
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
                            maxPackageIndex = 3 + int.Parse(package.Substring(currentStringIndex, length));
                            break;

                        default:
                            dataItemIndexes.Add(int.Parse(package.Substring(currentStringIndex, length)));
                            break;
                    }

                    currentStringIndex = nextStringIndex + 1;
                    currentPackageIndex++;
                }

                for (var i = 0; i < dataItemIndexes.Count; i++)
                {
                    dataItems.Add(i + 1 < dataItemIndexes.Count ? package.Substring(currentStringIndex + dataItemIndexes[i], dataItemIndexes[i + 1] - dataItemIndexes[i]) : package[(currentStringIndex + dataItemIndexes[i])..]);
                }

                DataItems = dataItems;

                IsValid = true;
            }
            catch (Exception)
            {
                IsValid = false;
            }
        }

        public Package(PackageType packageType, IReadOnlyList<string> dataItems, int origin = -1)
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
            DataItems = new List<string> { dataItem };
            IsValid = true;
        }

        public bool IsFullPackageData()
        {
            return PackageType == PackageType.GameData && DataItems.Count == 15 && !string.IsNullOrWhiteSpace(DataItems[4]);
        }

        public string ToString(string protocolVersion)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(protocolVersion);
            stringBuilder.Append("|");
            stringBuilder.Append((int) PackageType);
            stringBuilder.Append("|");
            stringBuilder.Append(Origin);
            stringBuilder.Append("|");
            stringBuilder.Append(DataItems.Count);

            var count = 0;

            foreach (var dataItem in DataItems)
            {
                stringBuilder.Append("|");
                stringBuilder.Append(count);

                count += dataItem.Length;
            }

            stringBuilder.Append("|");

            foreach (var dataItem in DataItems)
            {
                stringBuilder.Append(dataItem);
            }

            return stringBuilder.ToString();
        }
    }
}