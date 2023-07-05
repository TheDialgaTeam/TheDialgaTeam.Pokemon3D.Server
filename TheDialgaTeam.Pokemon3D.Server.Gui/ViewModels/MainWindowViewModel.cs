using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TheDialgaTeam.Microsoft.Extensions.Logging.Action;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Gui.ViewModels;

public sealed partial class MainWindowViewModel : ReactiveObject
{
    public ReactiveCommand<Unit, Unit> StartServerCommand { get; }

    public ReactiveCommand<Unit, Unit> StopServerCommand { get; }

    public ReactiveCommand<Unit, Unit> ExitCommand { get; }
    
    [Reactive]
    public string LogOutput { get; set; } = string.Empty;
    
    [ObservableAsProperty]
    public int LogOutputPosition { get; }

    [Reactive]
    private bool IsActive { get; set; }
    
    private readonly IPokemonServerListener _pokemonServerListener;

    private readonly object _logToConsoleLock = new();
    private readonly StringBuilder _logEntries = new();

    public MainWindowViewModel()
    {
        _pokemonServerListener = Program.ServiceProvider.GetRequiredService<IPokemonServerListener>();

        var actionLoggerConfiguration = Program.ServiceProvider.GetRequiredService<ActionLoggerConfiguration>();
        actionLoggerConfiguration.RegisteredActionLogger = LogToConsole;

        StartServerCommand = ReactiveCommand.CreateFromTask(StartServer, this.WhenAnyValue(model => model.IsActive).Select(b => b == false));
        StopServerCommand = ReactiveCommand.CreateFromTask(StopServer, this.WhenAnyValue(model => model.IsActive));
        ExitCommand = ReactiveCommand.CreateFromTask(Exit);

        this.WhenAnyValue(model => model.LogOutput).Select(s => s.Length).ToPropertyEx(this, model => model.LogOutputPosition);
    }

    [GeneratedRegex("\x1B(?:[@-Z\\-_]|\\[[0-?]*[ -/]*[@-~])")]
    private static partial Regex RemoveAnsiEscapeCode();

    private Task StartServer()
    {
        IsActive = true;
        return _pokemonServerListener.StartAsync(default);
    }

    private Task StopServer()
    {
        IsActive = false;
        return _pokemonServerListener.StopAsync(default);
    }

    private async Task Exit()
    {
        await StopServer();

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.TryShutdown();
        }
    }

    private void LogToConsole(string log)
    {
        lock (_logToConsoleLock)
        {
            var textToAppend = RemoveAnsiEscapeCode().Replace(log, string.Empty);
            var totalLength = _logEntries.Length + textToAppend.Length;
            
            if (totalLength > _logEntries.MaxCapacity)
            {
                _logEntries.Remove(0, totalLength - _logEntries.MaxCapacity);
            }

            _logEntries.Append(textToAppend);
            LogOutput = _logEntries.ToString();
        }
    }
}