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
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Localization;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Localization;

internal sealed class LocalizationProvider : ILocalization
{
    private readonly IPokemonServerOptions _options;

    public LocalizationProvider(IPokemonServerOptions options)
    {
        _options = options;
    }

    public string GetLocalizedString(Func<LocalizedToken, string> token, params object?[] args)
    {
        return string.Format(GetLocalizedTemplate(token), args);
    }

    public string GetLocalizedString(Func<LocalizedToken, string> token, GameDataPacket packet, params object?[] args)
    {
        var playerName = packet.IsGameJoltPlayer ? GetLocalizedString(t => t.PlayerNameDisplayFormat.GameJoltNameDisplayFormat, packet.Name, packet.GameJoltId) : GetLocalizedString(t => t.PlayerNameDisplayFormat.OfflineNameDisplayFormat, packet.Name);
        return GetLocalizedString(token, [playerName, ..args]);
    }

    private string GetLocalizedTemplate(Func<LocalizedToken, string> token)
    {
        var currentCulture = Thread.CurrentThread.CurrentCulture;

        do
        {
            if (_options.LocalizationOptions.CultureInfo.TryGetValue(currentCulture.Name, out var result))
            {
                return token(result);
            }

            currentCulture = currentCulture.Parent;
        } while (!currentCulture.Equals(CultureInfo.InvariantCulture));

        return string.Empty;
    }
}