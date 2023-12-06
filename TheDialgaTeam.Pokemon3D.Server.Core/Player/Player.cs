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
using Mediator;
using TheDialgaTeam.Pokemon3D.Server.Core.Database.Queries;
using TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables;
using TheDialgaTeam.Pokemon3D.Server.Core.Localization.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Player;

internal sealed class Player : IPlayer, IDisposable
{
    public IPAddress RemoteIpAddress => _client.RemoteIpAddress;
    
    public int Id => _gameDataPacket.Origin;
    public string GameMode => _gameDataPacket.GameMode;
    public bool IsGameJoltPlayer => _gameDataPacket.IsGameJoltPlayer;
    public string GameJoltId => _gameDataPacket.GameJoltId;
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

    public string DisplayName => _gameDataPacket.IsGameJoltPlayer ? _stringLocalizer[s => s.PlayerNameDisplayFormat.GameJoltNameDisplayFormat, Name, GameJoltId] : _stringLocalizer[s => s.PlayerNameDisplayFormat.OfflineNameDisplayFormat, Name];
    
    public bool IsReady { get; private set; }
    
    public PlayerProfile? PlayerProfile { get; private set; }

    public bool HasLocalWorld => _localWorld is not null;

    private readonly IStringLocalizer _stringLocalizer;
    private readonly IMediator _mediator;
    private readonly ILocalWorldFactory _localWorldFactory;
    private readonly IPokemonServerClient _client;
    private GameDataPacket _gameDataPacket;

    private ILocalWorld? _localWorld;

    public Player(IStringLocalizer stringLocalizer, IMediator mediator, ILocalWorldFactory localWorldFactory, IPokemonServerClient client, int id, GameDataPacket gameDataPacket)
    {
        _stringLocalizer = stringLocalizer;
        _mediator = mediator;
        _localWorldFactory = localWorldFactory;
        _client = client;
        _gameDataPacket = gameDataPacket with { Origin = id };
    }

    public async ValueTask InitializePlayer(ILocalWorld globalWorld, CancellationToken cancellationToken)
    {
        if (IsGameJoltPlayer)
        {
            PlayerProfile = await _mediator.Send(new GetPlayerProfile(this), cancellationToken).ConfigureAwait(false);
        }

        _localWorld = _localWorldFactory.CreateLocalWorld(globalWorld, this);
        _localWorld.StartWorld();
        
        IsReady = true;
    }

    public ValueTask ApplyGameDataAsync(RawPacket rawPacket)
    {
        _gameDataPacket = new GameDataPacket(this, rawPacket);
        return _mediator.Publish(new PlayerUpdated(this));
    }

    public GameDataPacket ToGameDataPacket()
    {
        return _gameDataPacket;
    }

    public void SendPacket(RawPacket rawPacket)
    {
        _client.SendPacket(rawPacket);
    }

    public ValueTask KickAsync(string reason)
    {
        return _client.KickAsync(reason);
    }

    public ValueTask DisconnectAsync(string? reason = null)
    {
        return _client.DisconnectAsync(reason);
    }

    public void Dispose()
    {
        _localWorld?.Dispose();
    }
}