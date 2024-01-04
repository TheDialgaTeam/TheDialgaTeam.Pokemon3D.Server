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

using System.Net;
using Mediator;
using TheDialgaTeam.Pokemon3D.Server.Core.Database.Entities;
using TheDialgaTeam.Pokemon3D.Server.Core.Database.Queries;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Commands;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Player;

internal sealed class Player(
    IStringLocalizer stringLocalizer, 
    IMediator mediator, 
    IPokemonServerClient client, 
    int id, 
    GameDataPacket gameDataPacket) : IPlayer, IDisposable
{
    public int Id => _gameDataPacket.Origin;
    public string GameMode => _gameDataPacket.GameMode;
    public bool IsGameJoltPlayer => _gameDataPacket.IsGameJoltPlayer;
    public ulong GameJoltId => _gameDataPacket.GameJoltId;
    public string NumberDecimalSeparator => _gameDataPacket.NumberDecimalSeparator;
    public string Name => _gameDataPacket.Name;
    public string MapFile => _gameDataPacket.MapFile;
    public Position PlayerPosition => _gameDataPacket.PlayerPosition;
    public int PlayerFacing => _gameDataPacket.PlayerFacing;
    public bool IsMoving => _gameDataPacket.IsMoving;
    public string PlayerSkin => _gameDataPacket.PlayerSkin;
    public BusyType BusyType => _gameDataPacket.BusyType;
    public bool PokemonVisible => _gameDataPacket.PokemonVisible;
    public Position PokemonPosition => _gameDataPacket.PokemonPosition;
    public string PokemonSkin => _gameDataPacket.PokemonSkin;
    public int PokemonFacing => _gameDataPacket.PokemonFacing;
    
    public PlayerProfile? PlayerProfile { get; private set; }
    
    public bool IsReady => PlayerProfile is not null;
    
    public string DisplayName
    {
        get
        {
            if (IsReady)
            {
                return _gameDataPacket.IsGameJoltPlayer ? stringLocalizer[s => s.PlayerNameDisplayFormat.GameJoltNameDisplayFormat, PlayerProfile!.DisplayName, GameJoltId] : stringLocalizer[s => s.PlayerNameDisplayFormat.OfflineNameDisplayFormat, PlayerProfile!.DisplayName];
            }

            return _gameDataPacket.IsGameJoltPlayer ? stringLocalizer[s => s.PlayerNameDisplayFormat.GameJoltNameDisplayFormat, Name, GameJoltId] : stringLocalizer[s => s.PlayerNameDisplayFormat.OfflineNameDisplayFormat, Name];
        }
    }

    public IPAddress RemoteIpAddress => client.RemoteIpAddress;

    private GameDataPacket _gameDataPacket = gameDataPacket with { Origin = id };
    private ILocalWorld? _localWorld;

    public async Task InitializePlayer(CancellationToken cancellationToken)
    {
        _localWorld = await mediator.Send(new CreateLocalWorld(this), cancellationToken).ConfigureAwait(false);
        _localWorld.StartWorld();
    }

    public async Task<bool> AuthenticatePlayer(string? requestName = null, string? requestPassword = null, CancellationToken cancellationToken = default)
    {
        PlayerProfile = await mediator.Send(new GetPlayerProfile(this, requestName, requestPassword), cancellationToken).ConfigureAwait(false);
        return IsReady;
    }

    public ValueTask ApplyGameDataAsync(IRawPacket rawPacket)
    {
        _gameDataPacket = new GameDataPacket(this, rawPacket);
        return mediator.Publish(new PlayerUpdated(this));
    }

    public GameDataPacket ToGameDataPacket()
    {
        if (!IsReady) throw new Exception("Player is not ready.");
        return _gameDataPacket with { Name = PlayerProfile!.DisplayName };
    }

    public void SendPacket(IPacket packet)
    {
        client.SendPacket(packet);
    }

    public void SendPacket(IRawPacket rawPacket)
    {
        client.SendPacket(rawPacket);
    }

    public void Kick(string reason)
    {
        SendPacket(new KickPacket(reason));
        client.Disconnect(reason, true);
    }

    public void Dispose()
    {
        _localWorld?.Dispose();
    }
}