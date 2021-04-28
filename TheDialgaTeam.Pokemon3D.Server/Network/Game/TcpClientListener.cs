using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Options;
using TheDialgaTeam.Pokemon3D.Server.Players;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Network.Game
{
    internal class TcpClientListener
    {
        private readonly Logger _logger;
        private readonly IOptionsMonitor<GameNetworkOptions> _optionsMonitor;
        private readonly PlayerNetworkFactory _playerNetworkFactory;

        private TcpListener? _tcpListener;
        private bool _isRunning;

        private Task? _tcpListenerTask;

        public TcpClientListener(Logger logger, IOptionsMonitor<GameNetworkOptions> optionsMonitor, PlayerNetworkFactory playerNetworkFactory)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _playerNetworkFactory = playerNetworkFactory;
        }

        public void StartListening()
        {
            if (_isRunning) return;
            _isRunning = true;

            _logger.LogInformation("[Server] Starting listener", true);

            var gameNetworkOptions = _optionsMonitor.CurrentValue;

            try
            {
                _tcpListener = new TcpListener(IPAddress.Parse(gameNetworkOptions.BindIpAddress), gameNetworkOptions.Port);
                _tcpListener.Start();

                _logger.LogInformation("\u001b[32;1m[Server] Started listening on port {Port} for new players\u001b[0m", true, gameNetworkOptions.Port);

                _tcpListenerTask = Task.Factory.StartNew(() =>
                {
                    var tcpListener = _tcpListener;

                    while (_isRunning)
                    {
                        try
                        {
                            _playerNetworkFactory.CreatePlayerNetwork().StartNetwork(tcpListener.AcceptTcpClient());
                        }
                        catch (SocketException ex)
                        {
                            _logger.LogError(ex, "[Server] New client could not be accepted due to an error", true);
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.LogError(ex, "[Server] Listener is not running", true);
                            StopListening();
                        }
                    }
                }, TaskCreationOptions.LongRunning);
            }
            catch (FormatException e)
            {
                _logger.LogError(e, "[Server] Invalid IP Address to bind against", true);
                StopListening();
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError(e, "[Server] Invalid Port value to bind against", true);
                StopListening();
            }
            catch (SocketException e)
            {
                _logger.LogError(e, "[Server] Unable to bind the target network as the port might be in use", true);
                StopListening();
            }
        }

        public void StopListening()
        {
            if (!_isRunning) return;
            _isRunning = false;

            _logger.LogInformation("[Server] Stopping listener", true);

            _tcpListener?.Stop();
            _tcpListenerTask?.GetAwaiter().GetResult();

            _logger.LogInformation("[Server] Listener stopped", true);
        }
    }
}