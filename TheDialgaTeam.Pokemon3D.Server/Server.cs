using System.Threading.Tasks;
using TheDialgaTeam.Pokemon3D.Server.Network;
using TheDialgaTeam.Pokemon3D.Server.Network.Game;

namespace TheDialgaTeam.Pokemon3D.Server
{
    internal class Server
    {
        private readonly NatDevices _natDevices;
        private readonly TcpClientListener _tcpClientListener;

        public Server(NatDevices natDevices, TcpClientListener tcpClientListener)
        {
            _natDevices = natDevices;
            _tcpClientListener = tcpClientListener;
        }

        public async Task StartServerAsync()
        {
            await _natDevices.OpenPortAsync();
        }

        public void StopServer()
        {
            _natDevices.Dispose();
        }
    }
}