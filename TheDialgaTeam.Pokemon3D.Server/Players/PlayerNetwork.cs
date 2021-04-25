using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Options;
using TheDialgaTeam.Pokemon3D.Server.Packages;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Players
{
    internal class PlayerNetwork
    {
        private readonly Logger _logger;
        private readonly IOptionsMonitor<ServerOptions> _optionsMonitor;
        private readonly PlayerCollection _playerCollection;

        private readonly ConcurrentQueue<Package> _packageQueue = new();

        private TcpClient? _tcpClient;

        private Task? _readingTask;
        private Task? _writingTask;

        private bool _isActive;

        private string RemoteIpAddress => (_tcpClient?.Client.RemoteEndPoint as IPEndPoint)?.ToString() ?? string.Empty;

        public PlayerNetwork(Logger logger, IOptionsMonitor<ServerOptions> optionsMonitor, PlayerCollection playerCollection)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _playerCollection = playerCollection;
        }

        public void StartNetwork(TcpClient tcpClient)
        {
            if (_isActive) return;
            _isActive = true;

            _tcpClient = tcpClient;
            var stream = tcpClient.GetStream();

            _readingTask = Task.Factory.StartNew(() =>
            {
                using var streamReader = new StreamReader(stream, leaveOpen: true);

                try
                {
                    while (_isActive)
                    {
                        var rawData = streamReader.ReadLine();
                        if (rawData == null) break;

                        // TODO: Handle data
                    }
                }
                catch (OutOfMemoryException ex)
                {
                    _logger.LogError(ex, "[{IpAddress:l}] Unable to allocate buffer for the package data due to insufficient memory", true, RemoteIpAddress);
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "[{IpAddress:l}] Unable to read data from this network", true, RemoteIpAddress);
                }
                finally
                {
                    _isActive = false;
                }
            }, TaskCreationOptions.LongRunning);

            _writingTask = Task.Factory.StartNew(() =>
            {
                using var streamWriter = new StreamWriter(stream, leaveOpen: true);

                while (_isActive)
                {
                    if (_packageQueue.TryDequeue(out var package))
                    {
                        try
                        {
                            streamWriter.WriteLine(package.ToString());
                            streamWriter.Flush();
                        }
                        catch (IOException ex)
                        {
                            _logger.LogError(ex, "[{IpAddress:l}] Unable to send data from this network", true, RemoteIpAddress);
                        }
                    }

                    Thread.Sleep(1);
                }

                try
                {
                    while (_packageQueue.TryDequeue(out var package))
                    {
                        streamWriter.WriteLine(package.ToString());
                        streamWriter.Flush();
                    }
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "[{IpAddress:l}] Unable to send data from this network", true, RemoteIpAddress);
                }

                _tcpClient.Client.Shutdown(SocketShutdown.Send);
            }, TaskCreationOptions.LongRunning);
        }

        public async Task StopNetworkAsync()
        {
            if (!_isActive) return;
            _isActive = false;

            await _writingTask;
        }
    }
}