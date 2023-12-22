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

using System.Text;
using TheDialgaTeam.Pokemon3D.Server.Core.Commands.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Commands;

public sealed class CommandProcessor(IPlayerCommand[] playerCommands)
{
    private const string CommandPrefix = "/";

    [ThreadStatic]
    private static StringBuilder? t_stringBuilder;

    public bool TryExecuteCommand(ReadOnlySpan<char> message, IPlayer? player, out CommandExecuteResult result)
    {
        if (!message.StartsWith(CommandPrefix))
        {
            result = CommandExecuteResult.Fail(new ArgumentException("Message is not a command"));
            return false;
        }

        message = message[CommandPrefix.Length..];

        if (player is not null)
        {
            foreach (var playerCommand in playerCommands)
            {
                if (!message.StartsWith(playerCommand.Name)) continue;
                
            }
        }
        
        result = CommandExecuteResult.Fail(new ArgumentException("Command does not exists."));
        return false;
    }
    
    internal static IEnumerable<string> GetCommandArgs(string message, int startIndex = 0)
    {
        var skipPaddingSpace = true;
        var isQuotedString = false;
        var isEscapeString = false;

        t_stringBuilder ??= new StringBuilder();
        t_stringBuilder.Clear();

        for (var i = startIndex; i < message.Length; i++)
        {
            var character = message[i];

            if (skipPaddingSpace)
            {
                if (character == ' ') continue;
                skipPaddingSpace = false;
            }

            if (isQuotedString)
            {
                if (isEscapeString)
                {
                    if (character != '\"' && character != '\\')
                    {
                        throw new ArgumentException("Invalid escape sequence.", nameof(message));
                    }

                    t_stringBuilder.Append(character);
                    isEscapeString = false;
                    continue;
                }

                switch (character)
                {
                    case '\\':
                        isEscapeString = true;
                        continue;

                    case '\"':
                        yield return t_stringBuilder.ToString();
                        t_stringBuilder.Clear();

                        isQuotedString = false;
                        skipPaddingSpace = true;
                        continue;

                    default:
                        t_stringBuilder.Append(character);
                        continue;
                }
            }

            switch (character)
            {
                case '\"':
                    isQuotedString = true;
                    continue;

                case ' ':
                    yield return t_stringBuilder.ToString();
                    t_stringBuilder.Clear();
                    skipPaddingSpace = true;
                    break;

                default:
                    t_stringBuilder.Append(character);
                    break;
            }
        }

        if (t_stringBuilder.Length > 0)
        {
            yield return t_stringBuilder.ToString();
            t_stringBuilder.Clear();
        }
        
        if (t_stringBuilder.Capacity > 1024)
        {
            t_stringBuilder.Capacity = 1024;
        }
    }
}