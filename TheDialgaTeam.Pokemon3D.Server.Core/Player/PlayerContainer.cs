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
using Mediator;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Commands;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Queries;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Player;

public sealed class PlayerContainer :
    IQueryHandler<GetPlayerCount, int>,
    IQueryHandler<GetPlayerById, IPlayer>,
    IQueryHandler<GetServerInfoData, ServerInfoDataPacket>,
    ICommandHandler<CreateNewPlayer, IPlayer>
{
    private readonly IPokemonServerOptions _options;
    private readonly IPlayerFactory _playerFactory;
    private readonly ConcurrentDictionary<IPokemonServerClient, IPlayer> _players = new();

    private int _nextRunningId;
    private readonly ConcurrentQueue<int> _runningIdQueue = new();

    public PlayerContainer(IPokemonServerOptions options, IPlayerFactory playerFactory)
    {
        _options = options;
        _playerFactory = playerFactory;
    }

    public ValueTask<int> Handle(GetPlayerCount query, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(_players.Count);
    }

    public ValueTask<IPlayer> Handle(GetPlayerById query, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(_players.Values.Single(player => player.Id == query.Id));
    }

    public ValueTask<ServerInfoDataPacket> Handle(GetServerInfoData query, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new ServerInfoDataPacket(
            _players.Count,
            _options.ServerOptions.MaxPlayers,
            _options.ServerOptions.ServerName,
            _options.ServerOptions.ServerDescription,
            _players.Values.Select(player => player.DisplayName).ToArray()));
    }

    public ValueTask<IPlayer> Handle(CreateNewPlayer command, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(_players.GetOrAdd(command.Client, _playerFactory.CreatePlayer(GetNextRunningId(), command.GameDataPacket)));
    }

    private int GetNextRunningId()
    {
        return _runningIdQueue.TryDequeue(out var id) ? id : Interlocked.Increment(ref _nextRunningId);
    }
}