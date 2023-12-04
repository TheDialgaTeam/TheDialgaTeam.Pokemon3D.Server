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
using System.Reactive.Disposables;
using Terminal.Gui;

namespace TheDialgaTeam.Pokemon3D.Server.Cli.Scheduler;

internal sealed class TerminalScheduler : LocalScheduler
{
    public static readonly TerminalScheduler Default = new();

    private TerminalScheduler()
    {
    }

    public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        return dueTime == TimeSpan.Zero ? PostOnMainLoop() : PostOnMainLoopAsTimeout();
        
        IDisposable PostOnMainLoop()
        {
            var composite = new CompositeDisposable(2);
            var cancellation = new CancellationDisposable();

            Application.MainLoop.Invoke(() =>
            {
                if (!cancellation.Token.IsCancellationRequested)
                {
                    composite.Add(action(this, state));
                }
            });

            composite.Add(cancellation);
            return composite;
        }
        
        IDisposable PostOnMainLoopAsTimeout()
        {
            var composite = new CompositeDisposable(2);
            
            var timeout = Application.MainLoop.AddTimeout(dueTime, _ =>
            {
                composite.Add(action(this, state));
                return false;
            });
            
            composite.Add(Disposable.Create(timeout, static args => Application.MainLoop.RemoveTimeout(args)));
            return composite;
        }
    }
}