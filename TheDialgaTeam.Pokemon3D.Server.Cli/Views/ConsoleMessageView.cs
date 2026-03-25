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

using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using TheDialgaTeam.Pokemon3D.Server.Cli.ViewModels;

namespace TheDialgaTeam.Pokemon3D.Server.Cli.Views;

internal sealed class ConsoleMessageView : FrameView
{
    public ListView ConsoleView { get; }

    public TextField CommandInputField { get; }
    
    public ConsoleMessageViewModel ViewModel { get; }

    private readonly CompositeDisposable _disposable = new();

    public ConsoleMessageView(IServiceProvider serviceProvider)
    {
        Title = "Console Message";
        ViewModel = ActivatorUtilities.CreateInstance<ConsoleMessageViewModel>(serviceProvider);
        
        ConsoleView = new ListView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            ViewportSettings = ViewportSettingsFlags.HasScrollBars
        };
        
        ConsoleView.SetSource(ViewModel.ConsoleMessages);

        CommandInputField = new TextField
        {
            Y = Pos.Bottom(ConsoleView),
            Width = Dim.Fill()
        };

        Add(ConsoleView, CommandInputField);
        
        ViewModel.ConsoleMessages
            .ObserveCollectionChanges()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(_ =>
            {
            }).DisposeWith(_disposable);
    }

    protected override void Dispose(bool disposing)
    {
        _disposable.Dispose();
        base.Dispose(disposing);
    }
}