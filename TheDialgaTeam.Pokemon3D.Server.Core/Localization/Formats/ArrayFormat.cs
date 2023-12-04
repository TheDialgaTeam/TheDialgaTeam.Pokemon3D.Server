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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Localization.Formats;

internal sealed class ArrayFormat<T> : IFormattable
{
    private readonly T[] _array;
    private readonly string? _format;
    private readonly IFormatProvider? _formatProvider;

    public ArrayFormat(T[] array, string? format = null, IFormatProvider? formatProvider = null)
    {
        _array = array;
        _format = format;
        _formatProvider = formatProvider;
    }
    
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return string.Join(",", _array.Select(arg =>
        {
            if (arg is IFormattable spanFormattable)
            {
                return spanFormattable.ToString(_format, _formatProvider);
            }

            return arg?.ToString();
        }));
    }
}