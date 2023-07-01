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

using System.Net;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Clients;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Clients.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packages;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public sealed partial class PokemonServer
{
    private readonly List<TcpClientNetwork> _tcpClientNetworks = new();
    private readonly object _clientLock = new();

    private void AddClient(TcpClientNetwork network)
    {
        lock (_clientLock)
        {
            SubscribeTcpClientNetworkEvent(network);
            network.Start();
            _tcpClientNetworks.Add(network);
        }
    }

    private void RemoveClient(TcpClientNetwork network)
    {
        lock (_clientLock)
        {
            UnsubscribeTcpClientNetworkEvent(network);
            _tcpClientNetworks.Remove(network);
        }
    }

    private void SubscribeTcpClientNetworkEvent(TcpClientNetwork network)
    {
        network.NewPackageReceived += TcpClientNetworkOnNewPackageReceived;
        network.Disconnected += TcpClientNetworkOnDisconnected;
    }

    private void UnsubscribeTcpClientNetworkEvent(TcpClientNetwork network)
    {
        network.NewPackageReceived -= TcpClientNetworkOnNewPackageReceived;
        network.Disconnected -= TcpClientNetworkOnDisconnected;
    }

    private void TcpClientNetworkOnDisconnected(object? sender, DisconnectedEventArgs e)
    {
        RemoveClient(e.Network);
    }

    private void TcpClientNetworkOnNewPackageReceived(object? sender, NewPackageReceivedEventArgs e)
    {
        switch (e.Package.PackageType)
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
                PrintHandleServerDataRequest(e.Network.RemoteIpAddress);

                var serverInfoData = new Package(PackageType.ServerInfoData, new[]
                {
                    "0",
                    _options.ServerOptions.MaxPlayers.ToString(),
                    _options.ServerOptions.ServerName,
                    _options.ServerOptions.ServerDescription
                });

                e.Network.EnqueuePackage(serverInfoData);
                break;
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Trace, Message = "[{ipAddress}] Received ServerDataRequest Package")]
    private partial void PrintHandleServerDataRequest(IPAddress ipAddress);
}