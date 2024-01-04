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

using System.Globalization;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Player;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;

public sealed record GameDataPacket(
    Origin Origin,
    string GameMode,
    bool IsGameJoltPlayer,
    ulong GameJoltId,
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
    int PokemonFacing) : IPacket
{
    public GameDataPacket(IRawPacket rawPacket) : this(
        rawPacket.Origin,
        rawPacket.DataItems[0],
        int.Parse(rawPacket.DataItems[1], CultureInfo.InvariantCulture) == 1,
        ulong.Parse(rawPacket.DataItems[2], CultureInfo.InvariantCulture),
        rawPacket.DataItems[3],
        rawPacket.DataItems[4],
        rawPacket.DataItems[5],
        Position.FromRawPacket(rawPacket.DataItems[6], rawPacket.DataItems[3]),
        int.Parse(rawPacket.DataItems[7], CultureInfo.InvariantCulture),
        int.Parse(rawPacket.DataItems[8], CultureInfo.InvariantCulture) == 1,
        rawPacket.DataItems[9],
        Enum.Parse<BusyType>(rawPacket.DataItems[10]),
        int.Parse(rawPacket.DataItems[11], CultureInfo.InvariantCulture) == 1,
        Position.FromRawPacket(rawPacket.DataItems[12], rawPacket.DataItems[3]),
        rawPacket.DataItems[13],
        int.Parse(rawPacket.DataItems[14], CultureInfo.InvariantCulture))
    {
    }

    public GameDataPacket(IPlayer player, IRawPacket rawPacket) : this(
        rawPacket.Origin,
        string.IsNullOrEmpty(rawPacket.DataItems[0]) ? player.GameMode : rawPacket.DataItems[0],
        string.IsNullOrEmpty(rawPacket.DataItems[1]) ? player.IsGameJoltPlayer : int.Parse(rawPacket.DataItems[1], CultureInfo.InvariantCulture) == 1,
        string.IsNullOrEmpty(rawPacket.DataItems[2]) ? player.GameJoltId : ulong.Parse(rawPacket.DataItems[2], CultureInfo.InvariantCulture),
        string.IsNullOrEmpty(rawPacket.DataItems[3]) ? player.NumberDecimalSeparator : rawPacket.DataItems[3],
        string.IsNullOrEmpty(rawPacket.DataItems[4]) ? player.Name : rawPacket.DataItems[4],
        string.IsNullOrEmpty(rawPacket.DataItems[5]) ? player.MapFile : rawPacket.DataItems[5],
        string.IsNullOrEmpty(rawPacket.DataItems[6]) ? player.PlayerPosition : Position.FromRawPacket(rawPacket.DataItems[6], string.IsNullOrEmpty(rawPacket.DataItems[3]) ? player.NumberDecimalSeparator : rawPacket.DataItems[3]),
        string.IsNullOrEmpty(rawPacket.DataItems[7]) ? player.PlayerFacing : int.Parse(rawPacket.DataItems[7], CultureInfo.InvariantCulture),
        string.IsNullOrEmpty(rawPacket.DataItems[8]) ? player.IsMoving : int.Parse(rawPacket.DataItems[8], CultureInfo.InvariantCulture) == 1,
        string.IsNullOrEmpty(rawPacket.DataItems[9]) ? player.PlayerSkin : rawPacket.DataItems[9],
        string.IsNullOrEmpty(rawPacket.DataItems[10]) ? player.BusyType : Enum.Parse<BusyType>(rawPacket.DataItems[10]),
        string.IsNullOrEmpty(rawPacket.DataItems[11]) ? player.PokemonVisible : int.Parse(rawPacket.DataItems[11], CultureInfo.InvariantCulture) == 1,
        string.IsNullOrEmpty(rawPacket.DataItems[12]) ? player.PokemonPosition : Position.FromRawPacket(rawPacket.DataItems[12], string.IsNullOrEmpty(rawPacket.DataItems[3]) ? player.NumberDecimalSeparator : rawPacket.DataItems[3]),
        string.IsNullOrEmpty(rawPacket.DataItems[13]) ? player.PokemonSkin : rawPacket.DataItems[13],
        string.IsNullOrEmpty(rawPacket.DataItems[14]) ? player.PokemonFacing : int.Parse(rawPacket.DataItems[14], CultureInfo.InvariantCulture))
    {
    }

    public static bool IsFullGameData(IRawPacket rawPacket)
    {
        return rawPacket.Origin == Origin.NewPlayer;
    }

    public IRawPacket ToServerRawPacket()
    {
        return new RawPacket(RawPacket.ProtocolVersion, PacketType.GameData, Origin, new[]
        {
            GameMode,
            IsGameJoltPlayer ? "1" : "0",
            GameJoltId.ToString(CultureInfo.InvariantCulture),
            NumberDecimalSeparator,
            Name,
            MapFile,
            PlayerPosition.ToRawPacketString(NumberDecimalSeparator),
            PlayerFacing.ToString(CultureInfo.InvariantCulture),
            IsMoving ? "1" : "0",
            PlayerSkin,
            ((int) BusyType).ToString(CultureInfo.InvariantCulture),
            PokemonVisible ? "1" : "0",
            PokemonPosition.ToRawPacketString(NumberDecimalSeparator),
            PokemonSkin,
            PokemonFacing.ToString(CultureInfo.InvariantCulture)
        });
    }

    public IRawPacket ToClientRawPacket()
    {
        return new RawPacket(RawPacket.ProtocolVersion, PacketType.GameData, Origin, new[]
        {
            GameMode,
            IsGameJoltPlayer ? "1" : "0",
            GameJoltId.ToString(CultureInfo.InvariantCulture),
            NumberDecimalSeparator,
            Name,
            MapFile,
            PlayerPosition.ToRawPacketString(NumberDecimalSeparator),
            PlayerFacing.ToString(CultureInfo.InvariantCulture),
            IsMoving ? "1" : "0",
            PlayerSkin,
            ((int) BusyType).ToString(CultureInfo.InvariantCulture),
            PokemonVisible ? "1" : "0",
            PokemonPosition.ToRawPacketString(NumberDecimalSeparator),
            PokemonSkin,
            PokemonFacing.ToString(CultureInfo.InvariantCulture)
        });
    }
}