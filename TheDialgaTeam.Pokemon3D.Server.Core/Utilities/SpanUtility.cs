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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

public static class SpanUtility
{
    public static ReadOnlySpan<char> SplitNext(this ReadOnlySpan<char> span, char separator, out ReadOnlySpan<char> next)
    {
        var nextIndex = span.IndexOf(separator);

        if (nextIndex == -1)
        {
            next = ReadOnlySpan<char>.Empty;
            return span;
        }

        next = span[(nextIndex + 1)..];
        return span[..nextIndex];
    }
    
    public static ReadOnlySpan<char> SplitNext(this ReadOnlySpan<char> span, string separator, out ReadOnlySpan<char> next)
    {
        var nextIndex = span.IndexOf(separator);

        if (nextIndex == -1)
        {
            next = ReadOnlySpan<char>.Empty;
            return span;
        }

        next = span[(nextIndex + separator.Length)..];
        return span[..nextIndex];
    }
}