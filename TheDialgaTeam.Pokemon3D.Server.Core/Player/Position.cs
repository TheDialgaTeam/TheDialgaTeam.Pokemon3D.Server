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

using System.Collections.Concurrent;
using System.Globalization;
using TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Player;

public readonly record struct Position(float X, float Y, float Z)
{
    private static readonly ConcurrentDictionary<string, NumberFormatInfo> NumberFormatInfos = new();
    
    public static Position FromRawPacket(ReadOnlySpan<char> position, string decimalSeparator)
    {
        var numberFormatInfo = NumberFormatInfos.GetOrAdd(decimalSeparator, static s => new NumberFormatInfo { NumberDecimalSeparator = s });
        return new Position(float.Parse(position.SplitNext("|", out position), numberFormatInfo), float.Parse(position.SplitNext("|", out position), numberFormatInfo), float.Parse(position.SplitNext("|", out position), numberFormatInfo));
    }

    public string ToRawPacketString(string decimalSeparator)
    {
        var numberFormatInfo = NumberFormatInfos.GetOrAdd(decimalSeparator, static s => new NumberFormatInfo { NumberDecimalSeparator = s });
        return $"{X.ToString(numberFormatInfo)}|{Y.ToString(numberFormatInfo)}|{Z.ToString(numberFormatInfo)}";
    }
}