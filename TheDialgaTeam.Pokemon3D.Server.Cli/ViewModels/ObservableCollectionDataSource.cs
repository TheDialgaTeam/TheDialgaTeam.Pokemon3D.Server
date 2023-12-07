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
using DynamicData;
using DynamicData.Binding;
using NStack;
using Terminal.Gui;

namespace TheDialgaTeam.Pokemon3D.Server.Cli.ViewModels;

public sealed class ObservableCollectionDataSource<T> : IListDataSource, IDisposable where T : notnull
{
    public int Count => _source.Count;

    public int Length { get; private set; }

    private readonly IList _source;
    private readonly CompositeDisposable _disposable = new();

    public ObservableCollectionDataSource(ObservableCollection<T> source)
    {
        _source = source;

        source
            .ToObservableChangeSet()
            .ToCollection()
            .Subscribe(collection => Length = collection.Count == 0 ? 0 : collection.Select(s => s.ToString()?.Length ?? 0).Max())
            .DisposeWith(_disposable);
    }

    private static void RenderUstr(View container, ConsoleDriver driver, ustring ustr, int col, int line, int width, int start = 0)
    {
        container.Move(col, line);
        
        var u = TextFormatter.ClipAndJustify(ustr.RuneSubstring(start), width, TextAlignment.Left);
        driver.AddStr(u);

        width -= TextFormatter.GetTextWidth(u);

        while (width-- > 0)
        {
            driver.AddRune(' ');
        }
    }

    public void Render(ListView container, ConsoleDriver driver, bool marked, int item, int col, int line, int width, int start = 0)
    {
        var t = _source[item];

        switch (t)
        {
            case null:
                RenderUstr(container, driver, ustring.Make(""), col, line, width, start);
                break;

            case ustring u:
                RenderUstr(container, driver, u, col, line, width, start);
                break;

            case string s:
                RenderUstr(container, driver, s, col, line, width, start);
                break;

            default:
                RenderUstr(container, driver, t.ToString() ?? string.Empty, col, line, width, start);
                break;
        }
    }

    public bool IsMarked(int item)
    {
        return false;
    }

    public void SetMark(int item, bool value)
    {
    }

    public IList ToList()
    {
        return _source;
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}