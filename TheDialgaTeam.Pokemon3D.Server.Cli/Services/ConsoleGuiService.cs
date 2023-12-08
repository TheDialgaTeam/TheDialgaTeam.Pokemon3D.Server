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

using System.Reactive.Concurrency;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Terminal.Gui;
using TheDialgaTeam.Pokemon3D.Server.Cli.Scheduler;
using TheDialgaTeam.Pokemon3D.Server.Cli.Views;

namespace TheDialgaTeam.Pokemon3D.Server.Cli.Services;

internal sealed class ConsoleGuiService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ConsoleGuiService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(() =>
        {
            Application.Init();
            Application.QuitKey = Key.F1;
            RxApp.MainThreadScheduler = TerminalScheduler.Default;
            RxApp.TaskpoolScheduler = TaskPoolScheduler.Default;
            Application.Run(new MainConsoleView(_serviceProvider));
        }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }
}