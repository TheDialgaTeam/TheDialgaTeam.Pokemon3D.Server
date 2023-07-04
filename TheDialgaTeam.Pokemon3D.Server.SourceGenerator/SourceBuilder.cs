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

using System.Reflection;
using System.Text;

namespace TheDialgaTeam.Pokemon3D.Server.SourceGenerator;

public sealed class SourceBuilder
{
    private readonly char _indentationChar;
    private readonly int _indentationAmount;
    private readonly char[] _indentationCache;

    private readonly StringBuilder _stringBuilder = new();

    private int _currentIndentation;

    public SourceBuilder(char indentationChar = ' ', int indentationAmount = 4)
    {
        _indentationChar = indentationChar;
        _indentationAmount = indentationAmount;
        _indentationCache = new char[indentationAmount * 5];
        _indentationCache.AsSpan().Fill(indentationChar);
    }

    public SourceBuilder WriteLine(string source)
    {
        AddIndentation();
        _stringBuilder.AppendLine(source);

        return this;
    }

    public SourceBuilder WriteEmptyLine()
    {
        _stringBuilder.AppendLine(string.Empty);
        return this;
    }

    public SourceBuilder WriteGeneratedCodeAttribute()
    {
        return WriteLine($"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{Assembly.GetExecutingAssembly().FullName}\", \"{Assembly.GetExecutingAssembly().GetName().Version}\")]");
    }

    public SourceBuilder WriteOpenBlock()
    {
        AddIndentation();

        _stringBuilder.AppendLine("{");
        _currentIndentation += _indentationAmount;

        return this;
    }

    public SourceBuilder WriteCloseBlock()
    {
        _currentIndentation -= _indentationAmount;

        AddIndentation();
        _stringBuilder.AppendLine("}");

        return this;
    }

    public override string ToString()
    {
        return _stringBuilder.ToString();
    }

    private void AddIndentation()
    {
        if (_currentIndentation <= _indentationCache.Length)
        {
            _stringBuilder.Append(_indentationCache.AsSpan(0, _currentIndentation).ToString());
        }
        else
        {
            for (var i = 0; i < _currentIndentation; i++)
            {
                _stringBuilder.Append(_indentationChar);
            }
        }
    }
}