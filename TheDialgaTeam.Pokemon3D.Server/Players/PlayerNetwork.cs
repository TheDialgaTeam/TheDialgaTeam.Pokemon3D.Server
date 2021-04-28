using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Options;
using TheDialgaTeam.Pokemon3D.Server.Packages;
using TheDialgaTeam.Pokemon3D.Server.Resources;
using TheDialgaTeam.Pokemon3D.Server.Serilog;
using Timer = System.Timers.Timer;

namespace TheDialgaTeam.Pokemon3D.Server.Players
{
    internal class PlayerNetwork : IDisposable
    {
        private readonly Logger _logger;
        private readonly IOptionsMonitor<ServerOptions> _optionsMonitor;
        private readonly PlayerCollection _playerCollection;

        private readonly TcpClient _tcpClient;
        private readonly string _remoteIpAddress;

        private readonly Task _writingTask;

        private readonly ConcurrentQueue<Package> _packageQueue = new();
        private readonly Timer _checkingTimer;

        private bool _isActive;

        private Player? _player;

        private DateTime _lastValidPackage = DateTime.Now;

        public PlayerNetwork(Logger logger, IOptionsMonitor<ServerOptions> optionsMonitor, PlayerCollection playerCollection, TcpClient tcpClient)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _playerCollection = playerCollection;

            _tcpClient = tcpClient;
            var stream = tcpClient.GetStream();

            _remoteIpAddress = (tcpClient.Client.RemoteEndPoint as IPEndPoint)!.ToString();

            _logger.LogDebug("[{IpAddress:l}] Starting network", true, _remoteIpAddress);
            _isActive = true;

            _ = Task.Factory.StartNew(() =>
            {
                using var streamReader = new StreamReader(stream, leaveOpen: true);

                try
                {
                    while (_isActive)
                    {
                        var rawData = streamReader.ReadLine();
                        if (rawData == null) break;

                        _logger.LogTrace("[{IpAddress:l}] Received raw package data: {RawData:l}", true, _remoteIpAddress, rawData);

                        HandlePackage(new Package(rawData));
                    }
                }
                catch (OutOfMemoryException ex)
                {
                    _logger.LogError(ex, "[{IpAddress:l}] Unable to allocate buffer for the package data due to insufficient memory", true, _remoteIpAddress);
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "[{IpAddress:l}] Unable to read data from this network", true, _remoteIpAddress);
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
                            _logger.LogError(ex, "[{IpAddress:l}] Unable to send data from this network", true, _remoteIpAddress);
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
                        _logger.LogError(ex, "[{IpAddress:l}] Unable to send data from this network", true, _remoteIpAddress);
                    }
                }
            }, TaskCreationOptions.LongRunning);

            _checkingTimer = new Timer { AutoReset = true, Interval = TimeSpan.FromSeconds(1).TotalMilliseconds };
            _checkingTimer.Elapsed += CheckingTimerOnElapsed;
            _checkingTimer.Start();

            _logger.LogDebug("[{IpAddress:l}] Network started", true, _remoteIpAddress);
        }

        public void StopNetwork()
        {
            if (!_isActive) return;
            _isActive = false;

            _logger.LogDebug("[{IpAddress:l}] Stopping network", true, _remoteIpAddress);

            _writingTask.GetAwaiter().GetResult();
            _tcpClient.Close();

            _logger.LogDebug("[{IpAddress:l}] Network Stopped", true, _remoteIpAddress);

            Dispose();
        }

        public void EnqueuePackage(Package package)
        {
            _packageQueue.Enqueue(package);
        }

        private void HandlePackage(Package package)
        {
            if (!package.IsValid)
            {
                _logger.LogDebug("[{IpAddress:l}] Invalid package received", true, _remoteIpAddress);
                if (_player == null) StopNetwork();
                return;
            }

            _lastValidPackage = DateTime.Now;

            var serverOptions = _optionsMonitor.CurrentValue;

            switch (package.PackageType)
            {
                case PackageType.GameData:
                {
                    if (_player == null)
                    {
                        // Expect full package data here.
                        if (!package.IsFullPackageData())
                        {
                            EnqueuePackage(new Package(PackageType.Kicked, string.Format(Localization.SERVER_KICKED, Localization.SERVER_INVALID_DATA)));
                            StopNetwork();
                        }
                        else
                        {
                            
                        }
                    }

                    break;
                }

                case PackageType.PrivateMessage:
                    break;

                case PackageType.ChatMessage:
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
                    EnqueuePackage(new Package(PackageType.ServerInfoData, new List<string> { _playerCollection.Players.Count.ToString(), serverOptions.MaxPlayers.ToString(), serverOptions.ServerName, serverOptions.ServerDescription }));
                    StopNetwork();
                    break;
                }

                default:
                {
                    _logger.LogDebug("[{IpAddress:l}] Unable to handle package due to unknown type", true, _remoteIpAddress);
                    break;
                }
            }
        }

        private void CheckingTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var serverOptions = _optionsMonitor.CurrentValue;

            if (DateTime.Now - _lastValidPackage > TimeSpan.FromMilliseconds(serverOptions.Network.Game.NoPingKickTime + serverOptions.Network.Game.MaxPingAllowed))
            {
                EnqueuePackage(new Package(PackageType.Kicked, string.Format(Localization.SERVER_KICKED, Localization.SERVER_NO_PING)));
                StopNetwork();
            }
        }

        public void Dispose()
        {
            _checkingTimer.Elapsed -= CheckingTimerOnElapsed;
            _checkingTimer.Dispose();

            _tcpClient.Dispose();
        }
    }
}