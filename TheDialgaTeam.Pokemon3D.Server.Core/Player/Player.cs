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

using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Player;

internal sealed class Player : IPlayer
{
    public int Id { get; }

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

    public string DisplayStatus => _gameDataPacket.IsGameJoltPlayer ? $"{Id}: {_gameDataPacket.Name} ({_gameDataPacket.GameJoltId}) - {_gameDataPacket.BusyType}" : $"{Id}: {_gameDataPacket.Name} - {_gameDataPacket.BusyType}";

    private GameDataPacket _gameDataPacket;

    public Player(int id, GameDataPacket gameDataPacket)
    {
        Id = id;
        _gameDataPacket = gameDataPacket with { Origin = id };
    }

    public void ApplyGameData(GameDataPacket gameDataPacket)
    {
        _gameDataPacket = gameDataPacket;
    }

    public GameDataPacket ToGameDataPacket()
    {
        return _gameDataPacket;
    }
}