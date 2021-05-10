using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Options.Server.Network;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Network
{
    internal class TcpClientListener
    {
        private readonly Logger _logger;
        private readonly IOptionsMonitor<GameNetworkOptions> _optionsMonitor;
        private readonly TcpClientCollection _tcpClientCollection;

        private bool _isRunning;

        private Thread? _tcpListenerThread;
        private TcpListener? _tcpListener;

        public TcpClientListener(Logger logger, IOptionsMonitor<GameNetworkOptions> optionsMonitor, TcpClientCollection tcpClientCollection)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _tcpClientCollection = tcpClientCollection;
        }

        public void StartListening()
        {
            if (_isRunning) return;
            _isRunning = true;

            _tcpListenerThread = new Thread(TcpListenerThreadStart) { Name = "TcpClient Listener", IsBackground = true };
            _tcpListenerThread.Start();
        }

        public void StopListening(bool waitToStop = true)
        {
            if (!_isRunning) return;
            _isRunning = false;

            _logger.LogInformation("[Server] Stopping listener", true);

            _tcpListener?.Stop();
            if (waitToStop) _tcpListenerThread?.Join();

            _logger.LogInformation("[Server] Listener stopped", true);
        }

        private void TcpListenerThreadStart()
        {
            try
            {
                var gameNetworkOptions = _optionsMonitor.CurrentValue;

                _logger.LogInformation("[Server] Starting listener", true);

                _tcpListener = new TcpListener(IPAddress.Parse(gameNetworkOptions.BindIpAddress), gameNetworkOptions.Port);
                _tcpListener.Start();

                _logger.LogInformation("\u001b[32;1m[Server] Started listening on port {Port} for new players\u001b[0m", true, gameNetworkOptions.Port);

                while (_isRunning)
                {
                    try
                    {
                        _tcpClientCollection.Add(_tcpListener.AcceptTcpClient());
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode != SocketError.Interrupted) _logger.LogError(ex, "[Server] New client could not be accepted due to an error", true);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.LogError(ex, "[Server] Listener is not running", true);
                        StopListening(false);
                    }
                }
            }
            catch (FormatException e)
            {
                _logger.LogError(e, "[Server] Invalid IP Address to bind against", true);
                StopListening(false);
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError(e, "[Server] Invalid Port value to bind against", true);
                StopListening(false);
            }
            catch (SocketException e)
            {
                _logger.LogError(e, "[Server] Unable to bind the target network as the port might be in use", true);
                StopListening(false);
            }
        }
    }
}