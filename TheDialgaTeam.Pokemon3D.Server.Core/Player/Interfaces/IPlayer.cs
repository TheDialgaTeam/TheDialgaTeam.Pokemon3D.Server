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
using TheDialgaTeam.Pokemon3D.Server.Core.Database.Entities;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Interfaces.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;

public interface IPlayer
{
    public int Id { get; }
    public string GameMode { get; }
    public bool IsGameJoltPlayer { get; }
    public ulong GameJoltId { get; }
    public string NumberDecimalSeparator { get; }
    public string Name { get; }
    public string MapFile { get; }
    public Position PlayerPosition { get; }
    public int PlayerFacing { get; }
    public bool IsMoving { get; }
    public string PlayerSkin { get; }
    public BusyType BusyType { get; }
    public bool PokemonVisible { get; }
    public Position PokemonPosition { get; }
    public string PokemonSkin { get; }
    public int PokemonFacing { get; }
    
    public PlayerProfile? PlayerProfile { get; }
    
    public bool IsReady { get; }
    
    public string DisplayName { get; }
    
    public IPAddress RemoteIpAddress { get; }

    public Task InitializePlayer(CancellationToken cancellationToken);

    public Task<bool> AuthenticatePlayer(string? requestName = null, string? requestPassword = null, CancellationToken cancellationToken = default);
    
    public ValueTask ApplyGameDataAsync(IRawPacket rawPacket);

    public GameDataPacket ToGameDataPacket();

    public void SendPacket(IPacket packet);

    public void SendPacket(IRawPacket rawPacket);

    public void Kick(string reason);
}