using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Serilog.Events;

namespace TheDialgaTeam.Pokemon3D.Server.Frontend.GUI.ViewModels
{
    internal class SettingsWindowViewModel : ReactiveObject
    {
        private string _serilogMinimumLevelDefault = string.Empty;

        public static string[] LogEventLevels => Enum.GetNames<LogEventLevel>();

        public string SerilogMinimumLevelDefault
        {
            get => _serilogMinimumLevelDefault;
            set => this.RaiseAndSetIfChanged(ref _serilogMinimumLevelDefault, value);
        }

        public SettingsWindowViewModel()
        {
        }

        public SettingsWindowViewModel(IHostEnvironment hostEnvironment)
        {
            var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(hostEnvironment.ContentRootPath, "appsettings.json")));
            //SerilogMinimumLevelDefault = settings.RootElement.GetProperty().["Serilog"]["MinimumLevel"]["Default"].Value<string>();
        }
    }
}