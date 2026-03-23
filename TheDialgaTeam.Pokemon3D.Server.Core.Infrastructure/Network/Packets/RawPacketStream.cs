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

using System.Text;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Network.Packets;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Packets;

public class RawPacketStream(Stream stream, int readBufferSize = 4096, int writeBufferSize = 4096)
{
    private readonly StreamReader _reader = new(stream, Encoding.UTF8, false, readBufferSize, true);
    private readonly StreamWriter _writer = new(stream, Encoding.UTF8, writeBufferSize, true);
    
    public async Task<IRawPacket?> ReadPacketAsync(CancellationToken cancellationToken = default)
    {
        return RawPacket.TryParse(await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false), out var packet) ? packet : null;
    }

    public void WritePacket(IRawPacket packet)
    {
        _writer.WriteLine(packet.ToRawPacketString());
        _writer.Flush();
    }
}