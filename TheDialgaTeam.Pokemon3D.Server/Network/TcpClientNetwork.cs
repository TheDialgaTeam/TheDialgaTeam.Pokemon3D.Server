using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Database;
using TheDialgaTeam.Pokemon3D.Server.Options.Server;
using TheDialgaTeam.Pokemon3D.Server.Packages;
using TheDialgaTeam.Pokemon3D.Server.Players;
using TheDialgaTeam.Pokemon3D.Server.Resources;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Network
{
    internal class TcpClientNetwork : IDisposable
    {
        private static readonly object TcpClientNetworkLock = new();

        private readonly Logger _logger;
        private readonly IOptionsMonitor<ServerOptions> _optionsMonitor;
        private readonly TcpClientCollection _tcpClientCollection;
        private readonly PlayerCollection _playerCollection;
        private readonly IDbContextFactory<SqliteDatabaseContext> _dbContextFactory;

        private readonly TcpClient _tcpClient;
        private readonly string _remoteIpAddress;

        private readonly Thread _readingThread;
        private readonly Thread _writingThread;

        private readonly Timer _checkingTimer;

        private readonly BlockingCollection<Package> _packages = new();

        private bool _isActive;

        private DateTime _lastValidPackage = DateTime.Now;

        private Player? _player;

        public TcpClientNetwork(Logger logger, IOptionsMonitor<ServerOptions> optionsMonitor, TcpClientCollection tcpClientCollection, PlayerCollection playerCollection, IDbContextFactory<SqliteDatabaseContext> dbContextFactory, TcpClient tcpClient)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _tcpClientCollection = tcpClientCollection;
            _playerCollection = playerCollection;
            _dbContextFactory = dbContextFactory;

            _tcpClient = tcpClient;

            _remoteIpAddress = (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.ToString() ?? string.Empty;

            _readingThread = new Thread(ReadingThreadStart) { Name = $"[{_remoteIpAddress}] Reading Thread", IsBackground = true };
            _writingThread = new Thread(WritingThreadStart) { Name = $"[{_remoteIpAddress}] Writing Thread", IsBackground = true };

            _checkingTimer = new Timer(CheckingTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void StartNetwork()
        {
            if (_isActive) return;
            _isActive = true;

            _logger.LogDebug("[{IpAddress:l}] Starting network", true, _remoteIpAddress);

            _readingThread.Start();
            _writingThread.Start();
            _checkingTimer.Change(0, 1000);

            _logger.LogDebug("[{IpAddress:l}] Network started", true, _remoteIpAddress);
        }

        public void StopNetwork()
        {
            lock (TcpClientNetworkLock)
            {
                if (!_isActive) return;
                _isActive = false;
            }

            _logger.LogDebug("[{IpAddress:l}] Stopping network", true, _remoteIpAddress);

            _writingThread.Join();

            _tcpClient.Close();
            _checkingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _packages.CompleteAdding();

            _logger.LogDebug("[{IpAddress:l}] Network Stopped", true, _remoteIpAddress);

            if (_player != null) _playerCollection.Remove(_player.Id);
            _tcpClientCollection.Remove(this);
        }

        public void EnqueuePackage(Package package)
        {
            try
            {
                _packages.Add(package);
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "[{IpAddress:l}] Package queue has been disposed", true, _remoteIpAddress);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "[{IpAddress:l}] Package queue is unable to accept new package", true, _remoteIpAddress);
            }
        }

        private void ReadingThreadStart()
        {
            using var streamReader = new StreamReader(_tcpClient.GetStream(), leaveOpen: true);

            while (_isActive)
            {
                try
                {
                    var rawData = streamReader.ReadLine();
                    if (rawData == null) break;

                    _logger.LogTrace("[{IpAddress:l}] Received raw package data: {RawData:l}", true, _remoteIpAddress, rawData);
                    HandlePackage(new Package(rawData));
                }
                catch (OutOfMemoryException)
                {
                    _logger.LogTrace("[{IpAddress:l}] \u001b[31;1mUnable to allocate buffer for the package data due to insufficient memory\u001b[0m", true, _remoteIpAddress);
                }
                catch (IOException)
                {
                    _logger.LogTrace("[{IpAddress:l}] \u001b[31;1mUnable to read data from this network\u001b[0m", true, _remoteIpAddress);
                }
            }
        }

        private void WritingThreadStart()
        {
            var serverOptions = _optionsMonitor.CurrentValue;
            using var streamWriter = new StreamWriter(_tcpClient.GetStream(), leaveOpen: true);

            try
            {
                while (_isActive)
                {

                    while (_packages.TryTake(out var package))
                    {
                        try
                        {
                            var packageData = package.ToString(serverOptions.ProtocolVersion);

                            streamWriter.WriteLine(packageData);
                            streamWriter.Flush();

                            _logger.LogTrace("[{IpAddress:l}] Sent raw package data: {RawData:l}", true, _remoteIpAddress, packageData);
                        }
                        catch (IOException)
                        {
                            _logger.LogTrace("[{IpAddress:l}] \u001b[31;1mUnable to send data from this network\u001b[0m", true, _remoteIpAddress);
                        }
                    }

                    Thread.Sleep(1);
                }

                while (_packages.TryTake(out var package))
                {
                    try
                    {
                        var packageData = package.ToString(serverOptions.ProtocolVersion);

                        streamWriter.WriteLine(packageData);
                        streamWriter.Flush();

                        _logger.LogTrace("[{IpAddress:l}] Sent raw package data: {RawData:l}", true, _remoteIpAddress, packageData);
                    }
                    catch (IOException)
                    {
                        _logger.LogTrace("[{IpAddress:l}] \u001b[31;1mUnable to send data from this network\u001b[0m", true, _remoteIpAddress);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void CheckingTimerCallback(object? state)
        {
            var serverOptions = _optionsMonitor.CurrentValue;

            if (DateTime.Now - _lastValidPackage > TimeSpan.FromMilliseconds(serverOptions.Network.Game.NoPingKickTime + serverOptions.Network.Game.MaxPingAllowed))
            {
                if (_player != null) EnqueuePackage(new Package(PackageType.Kicked, string.Format(Localization.SERVER_KICKED, Localization.SERVER_NO_PING)));
                StopNetwork();
            }
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
                            _player = _playerCollection.Add(this);

                            // Server Space Limit
                            if (_player == null)
                            {
                                EnqueuePackage(new Package(PackageType.Kicked, string.Format(Localization.SERVER_KICKED, Localization.SERVER_FULL)));
                                PlayerLog(package.DataItems[4], package.DataItems[2], $"is unable to join the server with the following reason: {Localization.SERVER_FULL}");
                                StopNetwork();
                                return;
                            }

                            var playerPacket = _player.Update(package);

                            // Offline mode
                            if (!serverOptions.OfflineMode && !_player.IsGameJoltPlayer)
                            {
                                EnqueuePackage(new Package(PackageType.Kicked, string.Format(Localization.SERVER_KICKED, Localization.SERVER_OFFLINE_MODE)));
                                PlayerLog(_player.Name, _player.GameJoltId, $"is unable to join the server with the following reason: {Localization.SERVER_OFFLINE_MODE}");
                                StopNetwork();
                                return;
                            }

                            // GameMode
                            if (Array.IndexOf(serverOptions.GameModes, _player.GameMode) < 0)
                            {
                                EnqueuePackage(new Package(PackageType.Kicked, string.Format(Localization.SERVER_KICKED, string.Format(Localization.SERVER_WRONG_GAMEMODE, string.Join(", ", serverOptions.GameModes)))));
                                PlayerLog(_player.Name, _player.GameJoltId, $"is unable to join the server with the following reason: {string.Format(Localization.SERVER_WRONG_GAMEMODE, string.Join(", ", serverOptions.GameModes))}");
                                StopNetwork();
                                return;
                            }

                            using (var context = _dbContextFactory.CreateDbContext())
                            {
                                // Blacklist
                                var blacklist = context.Blacklists.Find(_player.Name, _player.GameJoltId);

                                if (blacklist != null)
                                {
                                    EnqueuePackage(new Package(PackageType.Kicked, string.Format(Localization.SERVER_KICKED, string.Format(Localization.SERVER_BLACKLISTED, blacklist.Reason, blacklist.GetDurationRemaining()))));
                                    PlayerLog(_player.Name, _player.GameJoltId, $"is unable to join the server with the following reason: {string.Format(Localization.SERVER_BLACKLISTED, blacklist.Reason, blacklist.GetDurationRemaining())}");
                                    StopNetwork();
                                    return;
                                }
                            }

                            foreach (var (id, player) in _playerCollection.Players)
                            {
                                if (_player.Id == id) continue;

                                if (_player.IsGameJoltPlayer)
                                {
                                    if (_player.GameJoltId != player.GameJoltId) continue;

                                    EnqueuePackage(new Package(PackageType.Kicked, string.Format(Localization.SERVER_KICKED, Localization.SERVER_CLONE)));
                                    PlayerLog(_player.Name, _player.GameJoltId, $"is unable to join the server with the following reason: {Localization.SERVER_CLONE}");
                                    StopNetwork();
                                    return;
                                }

                                if (_player.Name != player.Name || player.IsGameJoltPlayer) continue;

                                EnqueuePackage(new Package(PackageType.Kicked, string.Format(Localization.SERVER_KICKED, Localization.SERVER_CLONE)));
                                PlayerLog(_player.Name, _player.GameJoltId, $"is unable to join the server with the following reason: {Localization.SERVER_CLONE}");
                                StopNetwork();
                                return;
                            }

                            // Okay, you are in :)
                            EnqueuePackage(new Package(PackageType.Id, _player.Id.ToString()));

                            foreach (var (id, player) in _playerCollection.Players)
                            {
                                if (id == _player.Id) continue;

                                EnqueuePackage(new Package(PackageType.CreatePlayer, id.ToString()));
                                EnqueuePackage(new Package(PackageType.GameData, player.GenerateGameData(), id));
                            }

                            _playerCollection.SendToAllPlayers(new Package(PackageType.CreatePlayer, _player.Id.ToString()));
                            _playerCollection.SendToAllPlayers(new Package(PackageType.GameData, playerPacket, _player.Id));

                            if (!string.IsNullOrWhiteSpace(serverOptions.WelcomeMessage))
                            {
                                EnqueuePackage(new Package(PackageType.ChatMessage, serverOptions.WelcomeMessage));
                            }

                            _playerCollection.SendToAllPlayers(new Package(PackageType.ChatMessage, _player.IsGameJoltPlayer ? string.Format(Localization.SERVER_GAMEJOLT, _player.Name, _player.GameJoltId, "join the game!") : string.Format(Localization.SERVER_NO_GAMEJOLT, _player.Name, "join the game!")));
                            PlayerLog(_player.Name, _player.GameJoltId, "join the game!");
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

        private void PlayerLog(string name, string gameJoltId, string message)
        {
            if (gameJoltId == string.Empty)
            {
                _logger.LogInformation(Localization.LOGGER_NO_GAMEJOLT, true, name, message);
            }
            else
            {
                _logger.LogInformation(Localization.LOGGER_GAMEJOLT, true, name, gameJoltId, message);
            }
        }

        public void Dispose()
        {
            _tcpClient.Dispose();
            _checkingTimer.Dispose();
            _packages.Dispose();
        }
    }
}