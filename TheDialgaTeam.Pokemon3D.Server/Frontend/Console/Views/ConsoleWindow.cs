using System.Threading.Tasks;
using TheDialgaTeam.Pokemon3D.Server.Frontend.Console.ViewModels;

namespace TheDialgaTeam.Pokemon3D.Server.Frontend.Console.Views
{
    internal class ConsoleWindow
    {
        private readonly ConsoleWindowViewModel _consoleWindowViewModel;

        private bool _isInitialized;

        public ConsoleWindow(ConsoleWindowViewModel consoleWindowViewModel)
        {
            _consoleWindowViewModel = consoleWindowViewModel;
        }

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _ = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var command = System.Console.ReadLine();
                    if (command == null) break;
                }
            }, TaskCreationOptions.LongRunning);

            _consoleWindowViewModel.StartServer();
        }
    }
}