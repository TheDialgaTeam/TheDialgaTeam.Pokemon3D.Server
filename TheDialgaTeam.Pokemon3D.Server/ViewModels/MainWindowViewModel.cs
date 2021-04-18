using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.ViewModels
{
    internal class MainWindowViewModel : ReactiveObject, IDisposable
    {
        private readonly ClassicDesktopStyleApplicationLifetime _classicDesktopStyleApplicationLifetime = null!;
        private readonly UiLogger _uiLogger = null!;
        private readonly Server _server = null!;

        private readonly List<string> _logOutputBuffer = new(1000);
        private string _logOutput = string.Empty;

        public string LogOutput
        {
            get => _logOutput;
            set => this.RaiseAndSetIfChanged(ref _logOutput, value);
        }

        public MainWindowViewModel()
        {
        }

        public MainWindowViewModel(ClassicDesktopStyleApplicationLifetime classicDesktopStyleApplicationLifetime, UiLogger uiLogger, Server server)
        {
            _classicDesktopStyleApplicationLifetime = classicDesktopStyleApplicationLifetime;
            _uiLogger = uiLogger;
            _server = server;

            uiLogger.Log += WriteToLogOutput;
        }

        public void StartServer()
        {
            Task.Run(async () => await _server.StartServerAsync());
        }

        public void StopServer()
        {
        }

        public void Exit()
        {
            _classicDesktopStyleApplicationLifetime.Shutdown();
        }

        private void WriteToLogOutput(string output)
        {
            if (_logOutputBuffer.Count == 1000)
            {
                _logOutputBuffer.RemoveAt(0);
            }

            _logOutputBuffer.Add(output);

            LogOutput = string.Join("", _logOutputBuffer);
        }

        public void Dispose()
        {
            _uiLogger.Log -= WriteToLogOutput;
        }
    }
}