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

using TheDialgaTeam.Pokemon3D.Server.Core.Network.Implementations.Packets.Types;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Player;

internal sealed class Player
{
    public int Id { get; }

    /// <summary>
    ///     Get Player DataItem[0]
    /// </summary>
    public string GameMode { get; private set; } = string.Empty;

    /// <summary>
    ///     Get Player DataItem[1]
    /// </summary>
    public bool IsGameJoltPlayer { get; private set; }

    /// <summary>
    ///     Get Player DataItem[2]
    /// </summary>
    public string GameJoltId { get; private set; } = string.Empty;

    /// <summary>
    ///     Get Player DataItem[3]
    /// </summary>
    public string NumberDecimalSeparator { get; private set; } = string.Empty;

    /// <summary>
    ///     Get Player DataItem[4]
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    ///     Get Player DataItem[5]
    /// </summary>
    public string MapFile { get; private set; } = string.Empty;

    /// <summary>
    ///     Get Player DataItem[6]
    /// </summary>
    public Position Position { get; private set; }

    /// <summary>
    ///     Get Player DataItem[7]
    /// </summary>
    public int Facing { get; private set; }

    /// <summary>
    ///     Get Player DataItem[8]
    /// </summary>
    public bool IsMoving { get; private set; }

    /// <summary>
    ///     Get Player DataItem[9]
    /// </summary>
    public string Skin { get; private set; } = string.Empty;

    /// <summary>
    ///     Get Player DataItem[10]
    /// </summary>
    public BusyType BusyType { get; private set; }

    /// <summary>
    ///     Get Player DataItem[11]
    /// </summary>
    public bool PokemonVisible { get; private set; }

    /// <summary>
    ///     Get Player DataItem[12]
    /// </summary>
    public Position PokemonPosition { get; private set; }

    /// <summary>
    ///     Get/Set Player DataItem[13]
    /// </summary>
    public string PokemonSkin { get; private set; } = string.Empty;

    /// <summary>
    ///     Get/Set Player DataItem[14]
    /// </summary>
    public int PokemonFacing { get; private set; }

    public string DisplayStatus => IsGameJoltPlayer ? $"{Id}: {Name} ({GameJoltId}) - {BusyType}" : $"{Id}: {Name} - {BusyType}";

    public Player(int id)
    {
        Id = id;
    }

    public void ApplyGameData(GameDataPacket gameDataPacket)
    {
        if (gameDataPacket.GameMode is not null)
        {
            GameMode = gameDataPacket.GameMode;
        }
        
        if (gameDataPacket.IsGameJoltPlayer is not null)
        {
            IsGameJoltPlayer = gameDataPacket.IsGameJoltPlayer.Value;
        }

        if (gameDataPacket.GameJoltId is not null)
        {
            GameJoltId = gameDataPacket.GameJoltId;
        }

        NumberDecimalSeparator = gameDataPacket.NumberDecimalSeparator;

        Name = gameDataPacket.Name;
        MapFile = gameDataPacket.MapFile;
        Position = gameDataPacket.PlayerPosition;
        Facing = gameDataPacket.PlayerFacing;
        IsMoving = gameDataPacket.IsMoving;
        Skin = gameDataPacket.PlayerSkin;
        BusyType = gameDataPacket.BusyType;
        PokemonVisible = gameDataPacket.PokemonVisible;
        PokemonPosition = gameDataPacket.PokemonPosition;
        PokemonSkin = gameDataPacket.PokemonSkin;
        PokemonFacing = gameDataPacket.PokemonFacing;
    }
}