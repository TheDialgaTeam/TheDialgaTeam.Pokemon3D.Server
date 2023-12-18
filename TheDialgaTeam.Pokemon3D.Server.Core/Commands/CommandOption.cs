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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Commands;

public sealed record CommandOption(string Name, string Description, bool IsRequired, CommandOptionType Type, object? Value = null)
{
    public CommandOption ParseCommandOption(ReadOnlySpan<char> message, out ReadOnlySpan<char> remainder)
    {
        switch (Type)
        {
            case CommandOptionType.String:
            {
                return ParseStringCommandOption(message, out remainder);
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private CommandOption ParseStringCommandOption(ReadOnlySpan<char> message, out ReadOnlySpan<char> remainder)
    {
        var valueStartIndex = 0;
        var valueEndIndex = message.Length;

        for (var i = 0; i < message.Length; i++)
        {
            if (i == 0 && message[0] == '"')
            {
                valueStartIndex = 1;
                continue;
            }
            
            if (message[i] == '\\' && i + 1 < message.Length && message[i + 1] == '"')
            {
                i += 1;
                continue;
            }
            
            if (valueStartIndex == 0 && message[i] == ' ')
            {
                valueEndIndex = i;
                break;
            }
            
            if (valueStartIndex == 1 && message[i] == '"')
            {
                valueEndIndex = i;
                break;
            }
        }

        remainder = valueEndIndex == message.Length ? ReadOnlySpan<char>.Empty : message[(valueEndIndex + 1)..];
                
        return this with { Value = message[new Range(valueStartIndex, valueEndIndex)].ToString() };
    }
}