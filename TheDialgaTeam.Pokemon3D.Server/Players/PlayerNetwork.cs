using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private Player? _player;

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

            _logger.LogDebug("[{IpAddress:l}] Starting network", true, RemoteIpAddress);

            _readingTask = Task.Factory.StartNew(() =>
            {
                using var streamReader = new StreamReader(stream, leaveOpen: true);

                try
                {
                    while (_isActive)
                    {
                        var rawData = streamReader.ReadLine();
                        if (rawData == null) break;

                        _logger.LogTrace("[{IpAddress:l}] Received raw package data: {RawData:l}", true, RemoteIpAddress, rawData);

                        HandlePackage(new Package(rawData));
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
            }, TaskCreationOptions.LongRunning);
            _writingTask = Task.Factory.StartNew(() =>
            {
                using var streamWriter = new StreamWriter(stream, leaveOpen: true);
                var serverOptions = _optionsMonitor.CurrentValue;

                while (_isActive)
                {
                    if (_packageQueue.TryDequeue(out var package))
                    {
                        try
                        {
                            streamWriter.WriteLine(package.ToString(serverOptions.ProtocolVersion));
                            streamWriter.Flush();
                        }
                        catch (IOException ex)
                        {
                            _logger.LogError(ex, "[{IpAddress:l}] Unable to send data from this network", true, RemoteIpAddress);
                        }
                    }

                    Thread.Sleep(1);
                }

                while (_packageQueue.TryDequeue(out var package))
                {
                    try
                    {
                        streamWriter.WriteLine(package.ToString(serverOptions.ProtocolVersion));
                        streamWriter.Flush();
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "[{IpAddress:l}] Unable to send data from this network", true, RemoteIpAddress);
                    }
                }
            }, TaskCreationOptions.LongRunning);

            _logger.LogTrace("[{IpAddress:l}] Network started", true, RemoteIpAddress);
        }

        public void StopNetwork()
        {
            if (!_isActive) return;
            _isActive = false;

            _logger.LogDebug("[{IpAddress:l}] Stopping network", true, RemoteIpAddress);

            _readingTask?.GetAwaiter().GetResult();
            _writingTask?.GetAwaiter().GetResult();

            _packageQueue.Clear();

            _tcpClient?.Close();
            _tcpClient?.Dispose();

            _logger.LogDebug("[{IpAddress:l}] Network Stopped", true, RemoteIpAddress);
        }

        public void EnqueuePackage(Package package)
        {
            _packageQueue.Enqueue(package);
        }

        private void HandlePackage(Package package)
        {
            if (!package.IsValid)
            {
                _logger.LogDebug("[{IpAddress:l}] Invalid package received", true, RemoteIpAddress);
                if (_player == null) StopNetwork();
                return;
            }

            var serverOptions = _optionsMonitor.CurrentValue;

            switch (package.PackageType)
            {
                case PackageType.GameData:
                    break;

                case PackageType.PrivateMessage:
                    break;

                case PackageType.ChatMessage:
                    break;

                case PackageType.Kicked:
                    break;

                case PackageType.Id:
                    break;

                case PackageType.CreatePlayer:
                    break;

                case PackageType.DestroyPlayer:
                    break;

                case PackageType.ServerClose:
                    break;

                case PackageType.ServerMessage:
                    break;

                case PackageType.WorldData:
                    break;

                case PackageType.Ping:
                    break;

                case PackageType.GamestateMessage:
                    break;

                case PackageType.TradeRequest:
                    break;

                case PackageType.TradeJoin:
                    break;

                case PackageType.TradeQuit:
                    break;

                case PackageType.TradeOffer:
                    break;

                case PackageType.TradeStart:
                    break;

                case PackageType.BattleRequest:
                    break;

                case PackageType.BattleJoin:
                    break;

                case PackageType.BattleQuit:
                    break;

                case PackageType.BattleOffer:
                    break;

                case PackageType.BattleStart:
                    break;

                case PackageType.BattleClientData:
                    break;

                case PackageType.BattleHostData:
                    break;

                case PackageType.BattlePokemonData:
                    break;

                case PackageType.ServerDataRequest:
                {
                    EnqueuePackage(new Package(PackageType.ServerInfoData, new List<string> { "0", serverOptions.MaxPlayers.ToString(), serverOptions.ServerName, serverOptions.ServerDescription }));
                    StopNetwork();
                    break;
                }

                default:
                    _logger.LogDebug("[{IpAddress:l}] Unable to handle package due to unknown type", true, RemoteIpAddress);
                    break;
            }
        }
    }
}