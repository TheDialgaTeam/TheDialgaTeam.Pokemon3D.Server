using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TheDialgaTeam.Pokemon3D.Server.Players
{
    internal class Player
    {
        private readonly TcpClient _tcpClient;

        private readonly StreamReader _streamReader;
        private readonly StreamWriter _streamWriter;

        private bool _isActive;

        private Task? _readingTask;
        private Task? _writingTask;

        public int Id { get; }

        public Player(TcpClient tcpClient, int id)
        {
            _tcpClient = tcpClient;
            _isActive = true;
            Id = id;
        }

        private void CreateReadingTask()
        {
            _readingTask = Task.Factory.StartNew(() =>
            {
                while (_isActive)
                {
                    var data = _streamReader.ReadLine();
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}