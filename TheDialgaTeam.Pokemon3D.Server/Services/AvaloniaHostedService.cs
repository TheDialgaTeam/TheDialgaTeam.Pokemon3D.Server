using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.Hosting;

namespace TheDialgaTeam.Pokemon3D.Server.Services
{
    internal class AvaloniaHostedService : IHostedService, IDisposable
    {
        private readonly ClassicDesktopStyleApplicationLifetime _classicDesktopStyleApplicationLifetime;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        private Task? _uiThreadTask;

        public AvaloniaHostedService(ClassicDesktopStyleApplicationLifetime classicDesktopStyleApplicationLifetime, IHostApplicationLifetime hostApplicationLifetime)
        {
            _classicDesktopStyleApplicationLifetime = classicDesktopStyleApplicationLifetime;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _uiThreadTask = Task.Factory.StartNew(() =>
            {
                Program.BuildAvaloniaApp().SetupWithLifetime(_classicDesktopStyleApplicationLifetime);
                _classicDesktopStyleApplicationLifetime.Start(_classicDesktopStyleApplicationLifetime.Args);
                _hostApplicationLifetime.StopApplication();
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Dispatcher.UIThread.Post(() => _classicDesktopStyleApplicationLifetime.Shutdown());
            if (_uiThreadTask != null) await _uiThreadTask;
        }

        public void Dispose()
        {
            _classicDesktopStyleApplicationLifetime.Dispose();
            _uiThreadTask?.Dispose();
        }
    }
}