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

namespace TheDialgaTeam.Pokemon3D.Server.Core.Domain.World;

public class WeatherSeason
{
    public Tuple<Weather, int>[] Winter => _winter;

    public Tuple<Weather, int>[] Spring => _spring;

    public Tuple<Weather, int>[] Summer => _summer;

    public Tuple<Weather, int>[] Fall => _fall;
    
    private Tuple<Weather, int>[] _winter = [];
    private Tuple<Weather, int>[] _spring = [];
    private Tuple<Weather, int>[] _summer = [];
    private Tuple<Weather, int>[] _fall = [];
    
    private static void CheckAndSetIfValid(ref Tuple<Weather, int>[] season, Tuple<Weather, int>[] weather)
    {
        season = weather.All(tuple => tuple.Item1 != Weather.WeatherSeason) ? weather : throw new ArgumentException("WeatherSeason array must have a valid weather.", nameof(weather));
    }
    
    private static Weather GetRandom(Tuple<Weather, int>[] array)
    {
        var sum = array.Sum(tuple => tuple.Item2);
        var count = 0;
        var target = Random.Shared.Next(0, sum);

        foreach (var tuple in array)
        {
            count += tuple.Item2;

            if (target < count)
            {
                return tuple.Item1;
            }
        }

        return array[^1].Item1;
    }

    public void Update(IReadOnlyList<Tuple<Weather, int>[]> weather)
    {
        if (weather.Count != 4) throw new ArgumentException("WeatherSeason array must have 4 entries.", nameof(weather));

        CheckAndSetIfValid(ref _winter, weather[0]);
        CheckAndSetIfValid(ref _spring, weather[1]);
        CheckAndSetIfValid(ref _summer, weather[2]);
        CheckAndSetIfValid(ref _fall, weather[3]);
    }

    public Weather GenerateNewWeather(Season season)
    {
        return season switch
        {
            Season.Winter => GetRandom(_winter),
            Season.Spring => GetRandom(_spring),
            Season.Summer => GetRandom(_summer),
            Season.Fall => GetRandom(_fall),
            var _ => throw new ArgumentOutOfRangeException(nameof(season), season, null)
        };
    }
}