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

using System.Globalization;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Player;

public readonly struct Position
{
    public double X { get; }

    public double Y { get; }

    public double Z { get; }

    public Position(string position, string decimalSeparator)
    {
        var positionString = position.Split('|');
        var numberFormatInfo = new NumberFormatInfo { NumberDecimalSeparator = decimalSeparator, NumberGroupSeparator = decimalSeparator == "." ? "," : "." };

        X = double.Parse(positionString[0], NumberStyles.Number, numberFormatInfo);
        Y = double.Parse(positionString[1], NumberStyles.Number, numberFormatInfo);
        Z = double.Parse(positionString[2], NumberStyles.Number, numberFormatInfo);
    }

    public override string ToString()
    {
        return $"{X}|{Y}|{Z}";
    }
}