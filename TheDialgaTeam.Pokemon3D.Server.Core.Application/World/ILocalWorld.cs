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

using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Domain.World;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Application.World;

public interface ILocalWorld : IDisposable
{
    Season CurrentSeason { get; }
    Weather CurrentWeather { get; }
    DateTime CurrentLocalTime { get; }
    
    bool DoDayCycle { get; }
    Season TargetSeason { get; }
    Weather TargetWeather { get; }
    TimeSpan TargetOffset { get; }
    SeasonMonthValue[][] TargetSeasonMonth { get; }
    WeatherSeasonValue[][] TargetWeatherSeason { get; }
    
    IObservable<ILocalWorld> ObserveLocalWorld { get; }

    void StartWorld();

    void StopWorld();

    void UpdateWorld(GameModeOverrideOptions options);
}