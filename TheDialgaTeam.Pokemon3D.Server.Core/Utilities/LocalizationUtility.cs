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
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Localization;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

public static class LocalizationUtility
{
    public static string GetLocalizedString(this IPokemonServerOptions options, Func<Token, string> format, params object?[] args)
    {
        var currentCulture = Thread.CurrentThread.CurrentCulture;

        do
        {
            if (options.LocalizationOptions.CultureInfo.TryGetValue(currentCulture.Name, out var token))
            {
                return string.Format(format(token), args);
            }

            currentCulture = currentCulture.Parent;
        } while (!currentCulture.Equals(CultureInfo.InvariantCulture));
        
        return string.Empty;
    }

    public static string GetLocalizedString(this IPokemonServerOptions options, Func<Token, string> format, IPlayer player, params object?[] values)
    {
        var playerName = player.IsGameJoltPlayer ? GetLocalizedString(options, token => token.PlayerNameDisplayFormat.GameJoltNameDisplayFormat, player.Name, player.GameJoltId) : GetLocalizedString(options, token => token.PlayerNameDisplayFormat.OfflineNameDisplayFormat, player.Name);
        return GetLocalizedString(options, format, [playerName, ..values]);
    }
    
    public static string GetLocalizedString(this IPokemonServerOptions options, Func<Token, string> format, GameDataPacket packet, params object?[] values)
    {
        var playerName = packet.IsGameJoltPlayer ? GetLocalizedString(options, token => token.PlayerNameDisplayFormat.GameJoltNameDisplayFormat, packet.Name, packet.GameJoltId) : GetLocalizedString(options, token => token.PlayerNameDisplayFormat.OfflineNameDisplayFormat, packet.Name);
        return GetLocalizedString(options, format, [playerName, ..values]);
    }
}