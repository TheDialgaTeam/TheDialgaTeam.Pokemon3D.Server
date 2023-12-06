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

using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using NStack;
using ReactiveUI;
using Terminal.Gui;
using TheDialgaTeam.Pokemon3D.Server.Cli.ViewModels;

namespace TheDialgaTeam.Pokemon3D.Server.Cli.Views;

internal sealed class ConsoleMessageView : FrameView
{
    private class DynamicListWrapper : IListDataSource, IDisposable
    {
        public int Count => _source.Count;
        
        public int Length { get; private set; }
        
        private readonly IList _source;
        private readonly CompositeDisposable _disposable = new();
        
        public DynamicListWrapper(IList source)
        {
            _source = source;

            if (source is ObservableCollection<string> observableCollection)
            {
                observableCollection
                    .ToObservableChangeSet()
                    .ToCollection()
                    .Do(collection => Length = collection.Select(s => s.Length).Max())
                    .Subscribe()
                    .DisposeWith(_disposable);
            }
        }
        
        public void Render (ListView container, ConsoleDriver driver, bool marked, int item, int col, int line, int width, int start = 0)
        {
            container.Move (col, line);
            var t = _source[item];
            if (t == null) {
                RenderUstr (driver, ustring.Make (""), col, line, width);
            } else {
                if (t is ustring u) {
                    RenderUstr (driver, u, col, line, width, start);
                } else if (t is string s) {
                    RenderUstr (driver, s, col, line, width, start);
                } else {
                    RenderUstr (driver, t.ToString (), col, line, width, start);
                }
            }
        }
        
        public bool IsMarked (int item)
        {
            return false;
        }
        
        public void SetMark (int item, bool value)
        {
        }
        
        public IList ToList ()
        {
            return _source;
        }
        
        private void RenderUstr (ConsoleDriver driver, ustring ustr, int col, int line, int width, int start = 0)
        {
            var u = TextFormatter.ClipAndJustify (ustr, width, TextAlignment.Left);
            driver.AddStr (u);
            width -= TextFormatter.GetTextWidth (u);
            while (width-- > 0) {
                driver.AddRune (' ');
            }
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
    
    private readonly CompositeDisposable _disposable = new();

    public ConsoleMessageView(IServiceProvider serviceProvider)
    {
        Title = "Console Message";
        
        var viewModel = ActivatorUtilities.CreateInstance<ConsoleMessageViewModel>(serviceProvider);

        var consoleView = new ListView(new DynamicListWrapper(viewModel.ConsoleMessages))
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };

        var inputField = new TextField
        {
            Y = Pos.Bottom(consoleView),
            Width = Dim.Fill()
        };
        
        Add(consoleView, inputField);
        
        viewModel.ConsoleMessages
            .ToObservableChangeSet()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                if (consoleView.HasFocus)
                {
                    consoleView.SetNeedsDisplay();
                    consoleView.EnsureSelectedItemVisible();
                }
                else
                {
                    consoleView.MoveEnd();
                }
            }).DisposeWith(_disposable);
    }

    protected override void Dispose(bool disposing)
    {
        _disposable.Dispose();
        base.Dispose(disposing);
    }
}