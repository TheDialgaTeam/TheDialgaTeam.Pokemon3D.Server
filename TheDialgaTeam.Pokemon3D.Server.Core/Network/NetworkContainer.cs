// Pokemon 3D Server Client
// Copyright (C) 2024 Yong Jian Ming
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
using System.Diagnostics.CodeAnalysis;
using Mediator;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Domain.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Formats;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Commands;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network;

public sealed class NetworkContainer(
    ILogger<NetworkContainer> logger,
    IMediator mediator,
    IPokemonServerOptions options,
    IStringLocalizer stringLocalizer,
    IPlayerFactory playerFactory) :
    ICommandHandler<AddClient, bool>,
    INotificationHandler<ClientConnected>,
    INotificationHandler<ClientDisconnected>,
    INotificationHandler<NewPacketReceived>,
    INotificationHandler<PlayerJoined>,
    INotificationHandler<PlayerUpdated>,
    INotificationHandler<PlayerLeft>
{
    private readonly ILogger _logger = logger;

    private readonly ConcurrentDictionary<IPokemonServerClient, IPlayer?> _players = new();

    private int _nextRunningId = 1;
    private readonly SortedSet<int> _runningIds = [];
    private readonly object _runningIdLock = new();

    #region Client Events

    public ValueTask<bool> Handle(AddClient command, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(_players.TryAdd(command.PokemonServerClient, null));
    }
    
    public ValueTask Handle(ClientConnected notification, CancellationToken cancellationToken)
    {
        if (!_players.TryAdd(notification.PokemonServerClient, null))
        {
            notification.PokemonServerClient.Disconnect();
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(ClientDisconnected notification, CancellationToken cancellationToken)
    {
        if (notification.PokemonServerClient is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (_players.TryRemove(notification.PokemonServerClient, out var player))
        {
            if (player is not null)
            {
                lock (_runningIdLock)
                {
                    _runningIds.Add(player.Id);
                }

                return mediator.Publish(new PlayerLeft(player, notification.Reason), cancellationToken);
            }
        }

        return ValueTask.CompletedTask;
    }

    #endregion

    #region Packet Handler

    public ValueTask Handle(NewPacketReceived notification, CancellationToken cancellationToken)
    {
        return notification.RawPacket.PacketType switch
        {
            PacketType.GameData => HandleGameDataPacket(notification, cancellationToken),
            PacketType.ChatMessage => HandleChatMessage(notification, cancellationToken),
            PacketType.GamestateMessage => HandleGamestateMessage(notification, cancellationToken),
            PacketType.ServerDataRequest => HandleServerDataRequest(notification),
            var _ => ValueTask.CompletedTask
        };
    }

    private async ValueTask HandleGameDataPacket(NewPacketReceived notification, CancellationToken cancellationToken)
    {
        if (!GameDataPacket.IsFullGameData(notification.RawPacket))
        {
            if (!TryGetPlayerById(notification.RawPacket.Origin, out var player)) throw new InvalidOperationException("Player does not exist.");

            await player.ApplyGameDataAsync(notification.RawPacket).ConfigureAwait(false);
            return;
        }

        // This is a new player joining.
        var gameDataPacket = new GameDataPacket(notification.RawPacket);

        var newPlayer = playerFactory.CreatePlayer(notification.Network, GetNextRunningId(), gameDataPacket);
        await newPlayer.InitializePlayer(cancellationToken).ConfigureAwait(false);

        _players[notification.Network] = newPlayer;

        var playerCanJoin = true;
        var reason = string.Empty;

        // Check Server Space Limit.
        var playerCount = GetPlayersEnumerable().Count();
        var maxPlayers = options.ServerOptions.MaxPlayers == -1 ? int.MaxValue : options.ServerOptions.MaxPlayers;

        if (playerCount >= maxPlayers)
        {
            playerCanJoin = false;
            reason = stringLocalizer[s => s.GameMessageFormat.ServerIsFull];
        }

        // Check Profile Type.
        if (!options.ServerOptions.AllowOfflinePlayer && !gameDataPacket.IsGameJoltPlayer)
        {
            playerCanJoin = false;
            reason = stringLocalizer[s => s.GameMessageFormat.ServerOnlyAllowGameJoltProfile];
        }

        switch (options.ServerOptions.AllowAnyGameModes)
        {
            // Check GameMode
            case true when options.ServerOptions.BlacklistedGameModes.Any(s => gameDataPacket.GameMode.Equals(s, StringComparison.OrdinalIgnoreCase)):
            {
                playerCanJoin = false;
                reason = stringLocalizer[s => s.GameMessageFormat.ServerBlacklistedGameModes];
                break;
            }

            case false when !options.ServerOptions.WhitelistedGameModes.Any(s => gameDataPacket.GameMode.Equals(s, StringComparison.OrdinalIgnoreCase)):
            {
                playerCanJoin = false;
                reason = stringLocalizer[s => s.GameMessageFormat.ServerWhitelistedGameModes, new ArrayStringFormat<string>(options.ServerOptions.WhitelistedGameModes)];
                break;
            }
        }

        if (playerCanJoin)
        {
            await mediator.Publish(new PlayerJoined(newPlayer), cancellationToken).ConfigureAwait(false);
            return;
        }

        newPlayer.Kick(reason);
        _logger.LogInformation("{Message}", stringLocalizer[s => s.ConsoleMessageFormat.PlayerUnableToJoin, newPlayer.DisplayName, reason]);
    }

    private ValueTask HandleChatMessage(NewPacketReceived notification, CancellationToken cancellationToken)
    {
        foreach (var otherPlayer in GetPlayersEnumerable())
        {
            otherPlayer.SendPacket(notification.RawPacket);
        }

        return ValueTask.CompletedTask;
    }

    private ValueTask HandleGamestateMessage(NewPacketReceived notification, CancellationToken cancellationToken)
    {
        /*
        var gamestateMessagePacket = new GamestateMessagePacket(notification.RawPacket);
        var player = GetPlayerById(gamestateMessagePacket.Origin);

        foreach (var otherPlayer in GetPlayersEnumerable())
        {
            otherPlayer.SendPacket(new ChatMessagePacket(Origin.Server, stringLocalizer[s => s.GameMessageFormat.GameStateMessage, player.DisplayName, gamestateMessagePacket.Message]));
        }

        _logger.LogInformation("{Message}", stringLocalizer[s => s.ConsoleMessageFormat.GameStateMessage, player.DisplayName, gamestateMessagePacket.Message]);
        */
        return ValueTask.CompletedTask;
    }

    private ValueTask HandleServerDataRequest(NewPacketReceived notification)
    {
        notification.Network.SendPacket(new ServerInfoDataPacket(
            GetPlayersEnumerable().Count(),
            options.ServerOptions.MaxPlayers == -1 ? int.MaxValue : options.ServerOptions.MaxPlayers,
            options.ServerOptions.ServerName,
            options.ServerOptions.ServerDescription,
            GetPlayersEnumerable().OrderBy(player => player.DisplayName).Take(20).Select(player => player.DisplayName).ToArray()));

        return ValueTask.CompletedTask;
    }

    #endregion

    #region Player Events

    public async ValueTask Handle(PlayerJoined notification, CancellationToken cancellationToken)
    {
        var player = notification.Player;
        player.SendPacket(new PlayerIdPacket(player.Id));

        if (player.IsGameJoltPlayer)
        {
            player.SendPacket(new ChatMessagePacket(-1, stringLocalizer[s => s.GameMessageFormat.AuthenticateGameJoltProfile]));
            await player.AuthenticatePlayer(null, null, cancellationToken).ConfigureAwait(false);
        }
        
        /*
        foreach (var otherPlayer in GetPlayersEnumerable())
        {
            if (otherPlayer != player)
            {
                player.SendPacket(new CreatePlayerPacket(otherPlayer.Id));
                player.SendPacket(otherPlayer.ToGameDataPacket());
            }

            otherPlayer.SendPacket(new CreatePlayerPacket(player.Id));
            otherPlayer.SendPacket(player.ToGameDataPacket());
            otherPlayer.SendPacket(new ChatMessagePacket(-1, stringLocalizer[s => s.GameMessageFormat.PlayerJoin, player.DisplayName]));
        }
        
        foreach (var welcomeMessage in options.ServerOptions.WelcomeMessage)
        {
            player.SendPacket(new ChatMessagePacket(-1, welcomeMessage));
        }
        */

        _logger.LogInformation("{Message}", stringLocalizer[s => s.ConsoleMessageFormat.PlayerJoin, player.DisplayName]);
    }

    public ValueTask Handle(PlayerUpdated notification, CancellationToken cancellationToken)
    {
        var player = notification.Player;

        foreach (var otherPlayer in GetPlayersEnumerable(null, true))
        {
            otherPlayer.SendPacket(player.ToGameDataPacket());
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(PlayerLeft notification, CancellationToken cancellationToken)
    {
        var player = notification.Player;
        var reason = notification.Reason;

        foreach (var otherPlayer in GetPlayersEnumerable(null, true))
        {
            otherPlayer.SendPacket(new DestroyPlayerPacket(player.Id));
            otherPlayer.SendPacket(new ChatMessagePacket(-1, stringLocalizer[s => s.GameMessageFormat.PlayerLeft, player.DisplayName]));
        }

        _logger.LogInformation("{Message}", reason is null ? stringLocalizer[s => s.ConsoleMessageFormat.PlayerLeft, player.DisplayName] : stringLocalizer[s => s.ConsoleMessageFormat.PlayerLeftWithReason, player.DisplayName, notification.Reason]);

        if (notification.Player is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return ValueTask.CompletedTask;
    }

    #endregion

    #region Player Functions

    private int GetNextRunningId()
    {
        lock (_runningIdLock)
        {
            if (_runningIds.Count == 0) return _nextRunningId++;

            var id = _runningIds.Min;
            _runningIds.Remove(id);
            return id;
        }
    }

    private bool TryGetPlayerById(int id, [MaybeNullWhen(false)] out IPlayer player)
    {
        foreach (var (_, value) in _players)
        {
            if (value is null) continue;
            if (value.Id != id) continue;

            player = value;
            return true;
        }

        player = null;
        return false;
    }

    private IEnumerable<IPlayer> GetPlayersEnumerable(IPlayer? excludePlayer = null, bool includeNonReady = false)
    {
        foreach (var (_, player) in _players)
        {
            if (player is null) continue;
            if (player == excludePlayer) continue;
            if (!includeNonReady && !player.IsReady) continue;
            yield return player;
        }
    }

    private async Task AuthenticatePlayer(IPlayer player, CancellationToken cancellationToken)
    {
        if (player.IsGameJoltPlayer)
        {
            await player.AuthenticatePlayer(null, null, cancellationToken).ConfigureAwait(false);
        }
    }

    #endregion
}