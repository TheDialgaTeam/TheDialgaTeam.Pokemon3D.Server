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
using TheDialgaTeam.Pokemon3D.Server.Core.Player;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets.Types;

internal sealed record GameDataPacket : Packet
{
    public string GameMode
    {
        get => DataItems[0];
        set => DataItems[0] = value;
    }

    public bool IsGameJoltPlayer
    {
        get => int.Parse(DataItems[1], CultureInfo.InvariantCulture) == 1;
        set => DataItems[1] = value ? "1" : "0";
    }

    public string GameJoltId
    {
        get => DataItems[2];
        set => DataItems[2] = value;
    }
    
    public string NumberDecimalSeparator
    {
        get => DataItems[3];
        set => DataItems[3] = value;
    }

    public string Name
    {
        get => DataItems[4];
        set => DataItems[4] = value;
    }
    
    public string MapFile
    {
        get => DataItems[5];
        set => DataItems[5] = value;
    }

    public Position PlayerPosition
    {
        get => Position.FromRawPacket(DataItems[6], NumberDecimalSeparator);
        set => DataItems[6] = value.ToRawPacket(NumberDecimalSeparator);
    }

    public int PlayerFacing
    {
        get => int.Parse(DataItems[7], CultureInfo.InvariantCulture);
        set => DataItems[7] = value.ToString(CultureInfo.InvariantCulture);
    }
    
    public bool IsMoving
    {
        get => int.Parse(DataItems[8], CultureInfo.InvariantCulture) == 1;
        set => DataItems[8] = value ? "1" : "0";
    }

    public string PlayerSkin
    {
        get => DataItems[9];
        set => DataItems[9] = value;
    }

    public BusyType BusyType
    {
        get => Enum.Parse<BusyType>(DataItems[10]);
        set => DataItems[10] = ((int) value).ToString(CultureInfo.InvariantCulture);
    }
    
    public bool PokemonVisible
    {
        get => int.Parse(DataItems[11], CultureInfo.InvariantCulture) == 1;
        set => DataItems[11] = value ? "1" : "0";
    }
    
    public Position PokemonPosition
    {
        get => Position.FromRawPacket(DataItems[12], NumberDecimalSeparator);
        set => DataItems[12] = value.ToRawPacket(NumberDecimalSeparator);
    }
    
    public string PokemonSkin
    {
        get => DataItems[13];
        set => DataItems[13] = value;
    }
    
    public int PokemonFacing
    {
        get => int.Parse(DataItems[14], CultureInfo.InvariantCulture);
        set => DataItems[14] = value.ToString(CultureInfo.InvariantCulture);
    }

    public GameDataPacket(int origin, string[] dataItems) : base(PacketType.GameData, origin)
    {
        DataItems = dataItems;
    }
}