using System.Threading;
using TheDialgaTeam.Pokemon3D.Server.Frontend.Console.ViewModels;

namespace TheDialgaTeam.Pokemon3D.Server.Frontend.Console.Views
{
    internal class ConsoleWindow
    {
        private readonly ConsoleWindowViewModel _consoleWindowViewModel;
        private readonly Thread _consoleThread;

        private bool _isInitialized;

        public ConsoleWindow(ConsoleWindowViewModel consoleWindowViewModel)
        {
            _consoleWindowViewModel = consoleWindowViewModel;
            _consoleThread = new Thread(ConsoleThreadStart) { Name = "Console Thread", IsBackground = true };
        }

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _consoleThread.Start();
            _consoleWindowViewModel.StartServer();
        }

        private void ConsoleThreadStart()
        {
            var cancellationToken = _consoleWindowViewModel.CancellationToken;

            while (!cancellationToken.IsCancellationRequested)
            {
                var command = System.Console.ReadLine();
                if (command == null) break;
            }
        }
    }
}