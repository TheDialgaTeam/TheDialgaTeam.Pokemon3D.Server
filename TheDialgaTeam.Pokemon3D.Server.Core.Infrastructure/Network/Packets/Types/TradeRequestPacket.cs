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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Packets.Types;

public sealed record TradeRequestPacket(Origin Origin, Origin TradePartner) : IPacket
{
    public TradeRequestPacket(IRawPacket rawPacket) : this(rawPacket.Origin, int.Parse(rawPacket.DataItems[0], CultureInfo.InvariantCulture))
    {
    }
    
    public IRawPacket ToServerResponseRawPacket()
    {
        return new RawPacket(RawPacket.ProtocolVersion, PacketType.TradeRequest, Origin, []);
    }

    public IRawPacket ToClientResponseRawPacket()
    {
        return new RawPacket(RawPacket.ProtocolVersion, PacketType.TradeRequest, Origin, [TradePartner.ToRawString()]);
    }
}