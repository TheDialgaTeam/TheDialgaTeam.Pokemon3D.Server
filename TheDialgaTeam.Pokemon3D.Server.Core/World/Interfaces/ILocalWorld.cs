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

namespace TheDialgaTeam.Pokemon3D.Server.Core.World.Interfaces;

public interface ILocalWorld : IDisposable
{
    public Season CurrentSeason { get; }
    
    public Weather CurrentWeather { get; }
    
    public DateTime CurrentTime { get; }
    
    public bool DoDayCycle { get; set; }
    
    public Season TargetSeason { get; set; }
    
    public Weather TargetWeather { get; set; }
    
    public TimeSpan TargetOffset { get; set; }
    
    public IPlayer? Player { get; }

    public void StartWorld();

    public void StopWorld();

    public WorldDataPacket GetWorldDataPacket();
}