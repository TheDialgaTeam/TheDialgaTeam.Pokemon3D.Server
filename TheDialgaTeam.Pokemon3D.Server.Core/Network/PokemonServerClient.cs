// Pokemon 3D Server Client
// Copyright (C) 2023 Yong Jian Ming
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Mediator;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

internal sealed class PokemonServerClient : IPokemonServerClient, IDisposable
{
    public IPAddress RemoteIpAddress { get; }

    private readonly ILogger<PokemonServerClient> _logger;
    private readonly IPokemonServerOptions _options;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly IMediator _mediator;
    private readonly TcpClient _tcpClient;

    private readonly StreamWriter _streamWriter;
    private readonly object _streamWriterLock = new();

    private int _isActive = 1;
    private DateTime _lastValidPackage = DateTime.Now;

    private readonly BlockingCollection<RawPacket> _gameDataPacketQueue = new();

    public PokemonServerClient(
        ILogger<PokemonServerClient> logger, 
        IPokemonServerOptions options, 
        IStringLocalizer stringLocalizer,
        IMediator mediator, 
        TcpClient tcpClient)
    {
        _logger = logger;
        _options = options;
        _stringLocalizer = stringLocalizer;
        _mediator = mediator;
        _tcpClient = tcpClient;
        
        _streamWriter = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8, tcpClient.SendBufferSize, true);
        _streamWriter.AutoFlush = true;

        RemoteIpAddress = (_tcpClient.Client.RemoteEndPoint as IPEndPoint)!.Address;

        Task.Run(RunReadingTask);
        Task.Run(RunConnectionCheckTask);
        Task.Factory.StartNew(HandleGameDataPacketTask, TaskCreationOptions.LongRunning);
    }

    public void SendPacket(RawPacket rawPacket)
    {
        if (_isActive == 0) return;
        
        lock (_streamWriterLock)
        {
            if (!_tcpClient.Connected) return;
            
            try
            {
                var packageData = rawPacket.ToRawPacketString();
                _logger.LogTrace("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.ClientSendRawPacket, RemoteIpAddress, packageData]);
                _streamWriter.WriteLine(packageData);
            }
            catch
            {
                _logger.LogDebug("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.ClientWriteSocketIssue, RemoteIpAddress]);
                throw;
            }
        }
    }

    public ValueTask KickAsync(string reason)
    {
        if (_isActive == 0) return ValueTask.CompletedTask;
        SendPacket(new KickPacket(reason).ToRawPacket());
        return DisconnectAsync(reason);
    }

    public ValueTask DisconnectAsync(string? reason = null)
    {
        if (Interlocked.CompareExchange(ref _isActive, 0, 1) == 0) return ValueTask.CompletedTask;
        
        _tcpClient.Close();
        _gameDataPacketQueue.CompleteAdding();
        
        _logger.LogDebug("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.ClientDisconnected, RemoteIpAddress]);
        
        return _mediator.Publish(new ClientDisconnected(this, reason));
    }

    private async Task RunReadingTask()
    {
        using var streamReader = new StreamReader(_tcpClient.GetStream(), Encoding.UTF8, false, _tcpClient.ReceiveBufferSize, true);

        while (_isActive == 1)
        {
            try
            {
                var rawData = await streamReader.ReadLineAsync().ConfigureAwait(false);
                
                if (rawData == null)
                {
                    await DisconnectAsync().ConfigureAwait(false);
                    return;
                }

                _logger.LogTrace("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.ClientReceiveRawPacket, RemoteIpAddress, rawData]);
           
                if (!RawPacket.TryParse(rawData, out var rawPacket))
                {
                    _logger.LogDebug("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.ClientReceiveInvalidPacket, RemoteIpAddress]);
                }
                else
                {
                    HandleIncomingPacket(rawPacket);
                }
            }
            catch
            {
                _logger.LogDebug("{Message}", _stringLocalizer[s => s.ConsoleMessageFormat.ClientReadSocketIssue, RemoteIpAddress]);
                await DisconnectAsync().ConfigureAwait(false);
                return;
            }
        }
    }

    private async Task RunConnectionCheckTask()
    {
        while (_isActive == 1)
        {
            if ((DateTime.Now - _lastValidPackage).TotalSeconds > _options.ServerOptions.NoPingKickTime)
            {
                // Most likely disconnected, so let's destroy it.
                await DisconnectAsync().ConfigureAwait(false);
                return;
            }

            await Task.Delay(1000).ConfigureAwait(false);
        }
    }

    private void HandleIncomingPacket(RawPacket rawPacket)
    {
        _lastValidPackage = DateTime.Now;

        switch (rawPacket.PacketType)
        {
            case PacketType.GameData:
            {
                _gameDataPacketQueue.Add(rawPacket);
                break;
            }

            default:
            {
                Task.Run(() => _mediator.Publish(new NewPacketReceived(this, rawPacket)));
                break;
            }
        }
    }
    
    private async Task HandleGameDataPacketTask()
    {
        foreach (var rawPacket in _gameDataPacketQueue.GetConsumingEnumerable())
        {
            await _mediator.Publish(new NewPacketReceived(this, rawPacket)).ConfigureAwait(true);
        }
    }

    public void Dispose()
    {
        _streamWriter.Dispose();
        _tcpClient.Dispose();
        _gameDataPacketQueue.Dispose();
    }
}