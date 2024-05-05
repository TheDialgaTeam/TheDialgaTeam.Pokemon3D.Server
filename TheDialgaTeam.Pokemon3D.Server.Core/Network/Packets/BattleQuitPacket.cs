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
using TheDialgaTeam.Pokemon3D.Server.Core.Domain.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces.Packets;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;

public sealed record BattleQuitPacket(Origin Origin, Origin BattlePartner) : IPacket
{
    public BattleQuitPacket(IRawPacket rawPacket) : this(rawPacket.Origin, int.Parse(rawPacket.DataItems[0], CultureInfo.InvariantCulture))
    {
    }
    
    public IRawPacket ToServerRawPacket()
    {
        return new RawPacket(RawPacket.ProtocolVersion, PacketType.BattleQuit, Origin, Array.Empty<string>());
    }

    public IRawPacket ToClientRawPacket()
    {
        return new RawPacket(RawPacket.ProtocolVersion, PacketType.BattleQuit, Origin, new[] { BattlePartner.ToRawPacketString() });
    }
}