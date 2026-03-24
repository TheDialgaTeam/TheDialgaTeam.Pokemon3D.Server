// Pokemon 3D Server Client
// Copyright (C) 2026 Yong Jian Ming
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
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Player;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Packets.Types;

public sealed record PrivateMessagePacket(IPlayer? Player, Origin Origin, Origin ChatPartner, string Message) : IPacket
{
    public PrivateMessagePacket(IRawPacket rawPacket) : this(null, rawPacket.Origin, int.Parse(rawPacket.DataItems[0], CultureInfo.InvariantCulture), rawPacket.DataItems[1])
    {
    }

    public PrivateMessagePacket(IPlayer player, IRawPacket rawPacket) : this(player, rawPacket.Origin, int.Parse(rawPacket.DataItems[0], CultureInfo.InvariantCulture), rawPacket.DataItems[1])
    {
    }

    public IRawPacket ToServerResponseRawPacket(IPlayer player)
    {
        return player.Id == Origin ? new RawPacket(RawPacket.ProtocolVersion, PacketType.PrivateMessage, Origin, [ChatPartner.ToRawString(), Message]) : new RawPacket(RawPacket.ProtocolVersion, PacketType.PrivateMessage, Origin, [Message]);
    }

    public IRawPacket ToServerResponseRawPacket()
    {
        if (Player is null)
        {
            return new RawPacket(RawPacket.ProtocolVersion, PacketType.PrivateMessage, Origin, [Message]);
        }

        return Player.Id == Origin ? new RawPacket(RawPacket.ProtocolVersion, PacketType.PrivateMessage, Origin, [ChatPartner.ToRawString(), Message]) : new RawPacket(RawPacket.ProtocolVersion, PacketType.PrivateMessage, Origin, [Message]);
    }

    public IRawPacket ToClientResponseRawPacket()
    {
        return new RawPacket(RawPacket.ProtocolVersion, PacketType.PrivateMessage, Origin, [ChatPartner.ToRawString(), Message]);
    }
}