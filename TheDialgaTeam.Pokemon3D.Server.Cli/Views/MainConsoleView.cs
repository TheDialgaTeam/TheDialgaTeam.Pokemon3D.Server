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

using Terminal.Gui;
using TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

namespace TheDialgaTeam.Pokemon3D.Server.Cli.Views;

internal sealed class MainConsoleView : Window
{
    public MainConsoleView()
    {
        Title = $"{ApplicationUtility.Name} v{ApplicationUtility.Version} ({ApplicationUtility.FrameworkVersion})";

        var playerViewList = new PlayerViewList
        {
            Width = Dim.Percent(25),
            Height = Dim.Fill(1)
        };

        var chatMessage = new ChatMessage
        {
            X = Pos.Right(playerViewList),
            Width = Dim.Fill(),
            Height = Dim.Fill(2)
        };

        var statusBar = new StatusBar();
        
        Add(playerViewList, chatMessage, statusBar);
    }
}