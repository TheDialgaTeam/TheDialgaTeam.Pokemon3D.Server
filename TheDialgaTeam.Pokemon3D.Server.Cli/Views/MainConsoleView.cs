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

using DynamicData;
using Terminal.Gui;
using TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

namespace TheDialgaTeam.Pokemon3D.Server.Cli.Views;

internal sealed class MainConsoleView : Window
{
    public MainConsoleView(IServiceProvider serviceProvider)
    {
        Title = $"{ApplicationUtility.Name} v{ApplicationUtility.Version} ({ApplicationUtility.FrameworkVersion})";

        var playerViewList = new PlayerListView(serviceProvider)
        {
            Width = Dim.Percent(20),
            Height = Dim.Fill(1)
        };

        var consoleMessageView = new ConsoleMessageView(serviceProvider)
        {
            X = Pos.Right(playerViewList),
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };

        var statusItems = new[]
        {
            new StatusItem(Application.QuitKey, $"~{Enum.GetName(Application.QuitKey)}~ Quit", Application.Shutdown)
        };

        var statusBar = new StatusBar(statusItems)
        {
            Y = Pos.Bottom(playerViewList),
            Width = Dim.Fill()
        };
        
        Add(playerViewList, consoleMessageView, statusBar);
    }
}