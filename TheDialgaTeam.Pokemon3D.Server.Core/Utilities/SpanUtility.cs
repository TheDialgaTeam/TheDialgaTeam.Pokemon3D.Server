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
    public static SpanSeparatorEnumerator Split(this ReadOnlySpan<char> span, string separator)
    {
        return new SpanSeparatorEnumerator(span, separator);
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

public ref struct SpanSeparatorEnumerator
{
    public ReadOnlySpan<char> Current => _current;
    
    public SpanSeparatorEnumerator GetEnumerator() => this;
    
    private readonly string _separator;
    
    private ReadOnlySpan<char> _remaining;
    private ReadOnlySpan<char> _current;
    private bool _isDone;

    public SpanSeparatorEnumerator(ReadOnlySpan<char> buffer, string separator)
    {
        _remaining = buffer;
        _separator = separator;
        _current = default;
    }

    public bool MoveNext()
    {
        if (_isDone) return false;

        var nextIndex = _remaining.IndexOf(_separator);
        
        if (nextIndex == -1)
        {
            _current = _remaining;
            _remaining = ReadOnlySpan<char>.Empty;
            _isDone = true;
        }
        else
        {
            _current = _remaining[..nextIndex];
            _remaining = _remaining[(nextIndex + _separator.Length)..];
        }

        return true;
    }
}