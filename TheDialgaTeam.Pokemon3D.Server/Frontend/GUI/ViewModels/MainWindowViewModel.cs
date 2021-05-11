#pragma warning disable 8618

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using TheDialgaTeam.Pokemon3D.Server.Frontend.GUI.Views;
using TheDialgaTeam.Pokemon3D.Server.Players;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Frontend.GUI.ViewModels
{
    internal class MainWindowViewModel : ReactiveObject, IDisposable
    {
        private readonly ClassicDesktopStyleApplicationLifetime _classicDesktopStyleApplicationLifetime;
        private readonly ActionLogger _actionLogger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly GameServer _gameServer;
        private readonly PlayerCollection _playerCollection;

        private readonly List<string> _logOutputBuffer = new(1000);
        private string _logOutput = string.Empty;
        private int _logOutputPosition;

        public string LogOutput
        {
            get => _logOutput;
            set => this.RaiseAndSetIfChanged(ref _logOutput, value);
        }

        public int LogOutputPosition
        {
            get => _logOutputPosition;
            set => this.RaiseAndSetIfChanged(ref _logOutputPosition, value);
        }

        public ObservableCollection<Player> Players => _playerCollection.Players;

        public MainWindowViewModel()
        {
        }

        public MainWindowViewModel(ClassicDesktopStyleApplicationLifetime classicDesktopStyleApplicationLifetime, ActionLogger actionLogger, IHostEnvironment hostEnvironment, GameServer gameServer, PlayerCollection playerCollection)
        {
            _classicDesktopStyleApplicationLifetime = classicDesktopStyleApplicationLifetime;
            _actionLogger = actionLogger;
            _hostEnvironment = hostEnvironment;
            _gameServer = gameServer;
            _playerCollection = playerCollection;

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
            LogOutputPosition = LogOutput.Length;
        }

        public void Dispose()
        {
            _actionLogger.Log -= WriteToLogOutput;
        }
    }
}