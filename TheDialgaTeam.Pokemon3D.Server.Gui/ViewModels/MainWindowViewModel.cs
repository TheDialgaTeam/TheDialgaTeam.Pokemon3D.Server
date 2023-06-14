using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TheDialgaTeam.Microsoft.Extensions.Logging.Action;
using TheDialgaTeam.Pokemon3D.Server.Core.Network;

namespace TheDialgaTeam.Pokemon3D.Server.Gui.ViewModels;

public sealed partial class MainWindowViewModel : ReactiveObject
{
    [ObservableAsProperty]
    public string LogOutput { get; }

    [ObservableAsProperty]
    public int LogOutputPosition { get; }

    public ReactiveCommand<Unit, Unit> StartServerCommand { get; }

    public ReactiveCommand<Unit, Unit> StopServerCommand { get; }

    public ReactiveCommand<Unit, Unit> ExitCommand { get; }

    [Reactive]
    private bool IsActive { get; set; }

    private readonly PokemonServer _pokemonServer;

    private readonly object _logToConsoleLock = new();
    private readonly ObservableCollection<string> _logEntries = new();

    public MainWindowViewModel()
    {
        _pokemonServer = Program.ServiceProvider.GetRequiredService<PokemonServer>();
        
        var actionLoggerConfiguration = Program.ServiceProvider.GetRequiredService<ActionLoggerConfiguration>();
        actionLoggerConfiguration.RegisteredActionLogger = LogToConsole;

        StartServerCommand = ReactiveCommand.CreateFromTask(StartServer, this.WhenAnyValue(model => model.IsActive).Select(b => b == false));
        StopServerCommand = ReactiveCommand.CreateFromTask(StopServer, this.WhenAnyValue(model => model.IsActive));
        ExitCommand = ReactiveCommand.CreateFromTask(Exit);

        _logEntries.ToObservableChangeSet().ToCollection().Select(collection => RemoveAnsiEscapeCode().Replace(string.Join(string.Empty, collection), string.Empty)).ToPropertyEx(this, model => model.LogOutput);
        this.WhenValueChanged(model => model.LogOutput).Select(s => s?.Length ?? 0).ToPropertyEx(this, model => model.LogOutputPosition);
    }

    [GeneratedRegex("\x1B(?:[@-Z\\-_]|\\[[0-?]*[ -/]*[@-~])")]
    private static partial Regex RemoveAnsiEscapeCode();

    private Task StartServer()
    {
        IsActive = true;
        return _pokemonServer.StartAsync();
    }

    private Task StopServer()
    {
        IsActive = false;
        return _pokemonServer.StopAsync();
    }

    private async Task Exit()
    {
        await StopServer();
        Environment.Exit(0);
    }

    private void LogToConsole(string log)
    {
        lock (_logToConsoleLock)
        {
            _logEntries.Add(log);

            while (_logEntries.Count > 5000)
            {
                _logEntries.RemoveAt(0);
            }
        }
    }
}