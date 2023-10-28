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

using TheDialgaTeam.Pokemon3D.Server.Core.Player.Implementations;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Implementations.Packets.Types;

internal sealed record GameDataPacket(
    int Origin,
    string? GameMode,
    bool? IsGameJoltPlayer,
    string? GameJoltId,
    string NumberDecimalSeparator,
    string Name,
    string MapFile,
    Position PlayerPosition,
    int PlayerFacing,
    bool IsMoving,
    string PlayerSkin,
    BusyType BusyType,
    bool PokemonVisible,
    Position PokemonPosition,
    string PokemonSkin,
    int PokemonFacing) : Packet(PacketType.GameData, Origin)
{
    public bool IsFullPackageData()
    {
        return GameMode is not null && IsGameJoltPlayer is not null && GameJoltId is not null;
    }

    protected override string[] GetDataItems()
    {
        return new[]
        {
            GameMode ?? string.Empty,
            IsGameJoltPlayer.HasValue ? IsGameJoltPlayer.Value ? "1" : "0" : string.Empty,
            GameJoltId ?? string.Empty,
            IsFullPackageData() ? NumberDecimalSeparator : string.Empty,
            IsFullPackageData() ? Name : IsGameJoltPlayer.GetValueOrDefault(false) ? string.Empty : Name,
            MapFile,
            PlayerPosition.ToRawPacket(NumberDecimalSeparator),
            PlayerFacing.ToString(),
            IsMoving ? "1" : "0",
            PlayerSkin,
            ((int) BusyType).ToString(),
            PokemonVisible ? "1" : "0",
            PokemonPosition.ToRawPacket(NumberDecimalSeparator),
            PokemonSkin,
            PokemonFacing.ToString()
        };
    }
}