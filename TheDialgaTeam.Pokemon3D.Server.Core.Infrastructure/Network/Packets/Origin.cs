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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Packets;

public readonly record struct Origin(int Id) : IFormattable, IParsable<Origin>
{
    public static Origin Server => new(-1);
    
    public static Origin NewPlayer => new(0);
    
    public static implicit operator int(Origin origin)
    {
        return origin.Id;
    }
    
    public static implicit operator Origin(int id)
    {
        return new Origin(id);
    }

    public string ToRawString()
    {
        return Id.ToString(CultureInfo.InvariantCulture);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return Id.ToString(format, formatProvider);
    }

    public static Origin Parse(string s, IFormatProvider? provider)
    {
        return new Origin(int.Parse(s, provider));
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Origin result)
    {
        var parse = int.TryParse(s, provider, out var id);
        result = parse ? new Origin(id) : default;
        return parse;
    }
}