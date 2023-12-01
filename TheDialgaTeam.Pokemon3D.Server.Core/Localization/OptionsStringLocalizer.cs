﻿// Pokemon 3D Server Client
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
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Localization;

internal sealed class OptionsStringLocalizer : IStringLocalizer
{
    public string this[Func<LocalizedString, string> localizedString] => GetLocalizedTemplate(localizedString);

    public string this[Func<LocalizedString, string> localizedString, params object?[] args] => string.Format(GetLocalizedTemplate(localizedString), args);

    public string this[Func<LocalizedString, string> localizedString, GameDataPacket packet, params object?[] args]
    {
        get
        {
            var playerNameDisplay = packet.IsGameJoltPlayer ? this[s => s.PlayerNameDisplayFormat.GameJoltNameDisplayFormat, packet.Name, packet.GameJoltId] : this[s => s.PlayerNameDisplayFormat.OfflineNameDisplayFormat, packet.Name];
            return string.Format(GetLocalizedTemplate(localizedString), [playerNameDisplay, ..args]);
        }
    }

    public string this[Func<LocalizedString, string> localizedString, IPlayer player, params object?[] args] => string.Format(GetLocalizedTemplate(localizedString), [player.DisplayName, ..args]);

    private readonly IPokemonServerOptions _options;

    public OptionsStringLocalizer(IPokemonServerOptions options)
    {
        _options = options;
    }

    private string GetLocalizedTemplate(Func<LocalizedString, string> localizedString)
    {
        var currentCulture = CultureInfo.CurrentCulture;

        do
        {
            if (_options.LocalizationOptions.CultureInfo.TryGetValue(currentCulture.Name, out var result))
            {
                return localizedString(result);
            }

            currentCulture = currentCulture.Parent;
        } while (!currentCulture.Equals(CultureInfo.InvariantCulture));

        return string.Empty;
    }
}