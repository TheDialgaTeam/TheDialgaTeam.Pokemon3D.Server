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

using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TheDialgaTeam.Serilog.Sinks.Action;

namespace TheDialgaTeam.Pokemon3D.Server.Cli.ViewModels;

internal sealed class ConsoleMessageViewModel : ReactiveObject
{
    public ObservableCollection<string> ConsoleMessages { get; } = [];
    
    public ConsoleMessageViewModel(ActionSinkOptions options)
    {
        options.RegisteredActionLogger = RegisteredActionLogger;
    }

    private void RegisteredActionLogger(string message)
    {
        foreach (var line in message.AsSpan().EnumerateLines())
        {
            if (line.IsWhiteSpace()) continue;

            if (ConsoleMessages.Count > 5000)
            {
                ConsoleMessages.RemoveAt(0);
            }
            
            ConsoleMessages.Add(line.ToString());
        }
    }
}