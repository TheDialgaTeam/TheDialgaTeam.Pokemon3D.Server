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
    private readonly int _indentationAmount;
    private readonly char[] _indentationCache;

    private readonly StringBuilder _stringBuilder = new();

    private int _currentIndentation;

    public SourceBuilder(char indentationChar = ' ', int indentationAmount = 4) : this(indentationChar, indentationAmount, 0)
    {
    }
    
    private SourceBuilder(char indentationChar = ' ', int indentationAmount = 4, int currentIndentation = 0)
    {
        _indentationAmount = indentationAmount;
        _indentationCache = new char[Math.Min(indentationAmount * 5, 1024 / sizeof(char))];
        _indentationCache.AsSpan().Fill(indentationChar);
        _currentIndentation = currentIndentation;
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

    public SourceBuilder WriteUsingNamespace(params string[] namespaces)
    {
        foreach (var @namespace in namespaces)
        {
            WriteLine($"using {@namespace};");
        }

        WriteEmptyLine();
        
        return this;
    }
    
    public SourceBuilder WriteNamespace(string @namespace)
    {
        return WriteLine($"namespace {@namespace}");
    }

    public SourceBuilder WriteGeneratedCodeAttribute()
    {
        return WriteLine($"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{Assembly.GetExecutingAssembly().GetName().Name}\", \"{Assembly.GetExecutingAssembly().GetName().Version}\")]");
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

    public SourceBuilder WriteBlock(Action<SourceBuilder> action)
    {
        WriteOpenBlock();

        var sourceBuilder = new SourceBuilder(_indentationCache[0], _indentationAmount, _currentIndentation);
        action(sourceBuilder);
        _stringBuilder.Append(sourceBuilder);
        
        WriteCloseBlock();
        
        return this;
    }

    public override string ToString()
    {
        return _stringBuilder.ToString();
    }

    private void AddIndentation()
    {
        var indentationApplied = 0;

        do
        {
            var indentAmount = Math.Min(_currentIndentation - indentationApplied, _indentationCache.Length);
            _stringBuilder.Append(_indentationCache, 0, indentAmount);
            indentationApplied += indentAmount;
        } while (_currentIndentation != indentationApplied);
    }
}