using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using Serilog.Events;

namespace TheDialgaTeam.Pokemon3D.Server.Frontend.GUI.ViewModels
{
    internal class SettingsWindowViewModel : ReactiveObject
    {
        private string _serilogMinimumLevelDefault;

        public static string Test => "\uf26e";

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
            var settings = JObject.Parse(File.ReadAllText(Path.Combine(hostEnvironment.ContentRootPath, "appsettings.json")));

            SerilogMinimumLevelDefault = settings["Serilog"]["MinimumLevel"]["Default"].Value<string>();
        }
    }
}