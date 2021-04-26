using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using TheDialgaTeam.Pokemon3D.Server.Frontend.GUI.Views;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Frontend.GUI.ViewModels
{
    internal class MainWindowViewModel : ReactiveObject, IDisposable
    {
        private readonly ClassicDesktopStyleApplicationLifetime _classicDesktopStyleApplicationLifetime = null!;
        private readonly ActionLogger _actionLogger = null!;
        private readonly IHostEnvironment _hostEnvironment = null!;
        private readonly GameServer _gameServer = null!;

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

        public MainWindowViewModel(ClassicDesktopStyleApplicationLifetime classicDesktopStyleApplicationLifetime, ActionLogger actionLogger, IHostEnvironment hostEnvironment, GameServer gameServer)
        {
            _classicDesktopStyleApplicationLifetime = classicDesktopStyleApplicationLifetime;
            _actionLogger = actionLogger;
            _hostEnvironment = hostEnvironment;
            _gameServer = gameServer;

            actionLogger.Log += WriteToLogOutput;
        }

        public async void StartServer()
        {
            await _gameServer.StartServerAsync();
        }

        public void StopServer()
        {
            _gameServer.StopServer();
        }

        public void OpenSettings()
        {
            var window = new SettingsWindow
            {
                DataContext = new SettingsWindowViewModel(_hostEnvironment)
            };
            window.Show();
        }

        public void Exit()
        {
            _gameServer.StopServer();
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
            _actionLogger.Log -= WriteToLogOutput;
        }
    }
}