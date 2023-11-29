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

using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Localization;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options;

public sealed class LocalizationOptions : IDisposable
{
    public PlayerNameDisplayFormat PlayerNameDisplayFormat { get; private set; }
    
    public ServerMessageFormat ServerMessageFormat { get; private set; }
    
    public GameMessageFormat GameMessageFormat { get; private set; }
    
    private readonly IDisposable?[] _disposables;

    public LocalizationOptions(
        IOptionsMonitor<PlayerNameDisplayFormat> playerNameDisplayFormat,
        IOptionsMonitor<ServerMessageFormat> serverMessageFormat,
        IOptionsMonitor<GameMessageFormat> gameMessageFormat)
    {
        PlayerNameDisplayFormat = playerNameDisplayFormat.CurrentValue;
        ServerMessageFormat = serverMessageFormat.CurrentValue;
        GameMessageFormat = gameMessageFormat.CurrentValue;

        _disposables = new[]
        {
            playerNameDisplayFormat.OnChange(format => PlayerNameDisplayFormat = format),
            serverMessageFormat.OnChange(format => ServerMessageFormat = format),
            gameMessageFormat.OnChange(format => GameMessageFormat = format)
        };
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable?.Dispose();
        }
    }
}