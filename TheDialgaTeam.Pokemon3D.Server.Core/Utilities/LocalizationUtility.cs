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

using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

public static class LocalizationUtility
{
    public static string GetLocalizedString(this IPokemonServerOptions options, string token, params object?[] values)
    {
        if (options.LocalizationOptions.Global.TryGetValue(token, out var globalFormat))
        {
            return string.Format(globalFormat, values);
        }

        if (options.LocalizationOptions.ServerMessage.TryGetValue(token, out var serverMessageFormat))
        {
            return string.Format(serverMessageFormat, values);
        }

        if (options.LocalizationOptions.GameMessage.TryGetValue(token, out var gameMessageFormat))
        {
            return string.Format(gameMessageFormat, values);
        }

        return string.Empty;
    }

    public static string GetLocalizedString(this IPokemonServerOptions options, string token, IPlayer player, params object?[] values)
    {
        var playerName = player.IsGameJoltPlayer ? GetLocalizedString(options, "SERVER_GAMEJOLT_NAME_FORMAT", player.Name, player.GameJoltId) : GetLocalizedString(options, "SERVER_LOCAL_NAME_FORMAT", player.Name);
        return GetLocalizedString(options, token, [playerName, ..values]);
    }
    
    public static string GetLocalizedString(this IPokemonServerOptions options, string token, GameDataPacket packet, params object?[] values)
    {
        var playerName = packet.IsGameJoltPlayer ? GetLocalizedString(options, "SERVER_GAMEJOLT_NAME_FORMAT", packet.Name, packet.GameJoltId) : GetLocalizedString(options, "SERVER_LOCAL_NAME_FORMAT", packet.Name);
        return GetLocalizedString(options, token, [playerName, ..values]);
    }
}