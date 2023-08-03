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

using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.World;

internal sealed class World
{
    private static int WeekOfYear => (int) ((DateTime.Now.DayOfYear - (DateTime.Now.DayOfWeek - DayOfWeek.Monday)) / 7.0 + 1.0);

    private readonly IPokemonServerOptions _options;

    private Season _targetSeason;
    private Weather _targetWeather;

    private Season _currentSeason = Season.Spring;
    private Weather _currentWeather = Weather.Clear;

    public World(IPokemonServerOptions options)
    {
        _options = options;
        _targetSeason = options.WorldOptions.Season;
        _targetWeather = options.WorldOptions.Weather;
    }

    public World(IPokemonServerOptions options, Season season, Weather weather)
    {
        _options = options;
        _targetSeason = season;
        _targetWeather = weather;
    }

    private void GenerateNewSeason(Season targetSeason)
    {
        switch (targetSeason)
        {
            case Season.Default:
            {
                _currentSeason = (WeekOfYear % 4) switch
                {
                    0 => Season.Fall,
                    1 => Season.Winter,
                    2 => Season.Spring,
                    3 => Season.Summer,
                    var _ => _currentSeason
                };
                break;
            }

            case Season.Random:
            {
                _currentSeason = (Season) Random.Shared.Next(0, 4);
                break;
            }

            case Season.SeasonMonth:
            {
                GenerateNewSeason((Season) _options.WorldOptions.SeasonMonth[DateTime.Now.Month]);
                break;
            }

            default:
            {
                _currentSeason = targetSeason;
                break;
            }
        }
    }

    private void GenerateNewWeather(Weather targetWeather)
    {
        switch (targetWeather)
        {
            case Weather.Default:
            {
                var random = Random.Shared.Next(0, 100);

                _currentWeather = _currentSeason switch
                {
                    Season.Winter => random switch
                    {
                        < 30 => Weather.Clear,
                        < 40 => Weather.Rain,
                        var _ => Weather.Snow
                    },
                    Season.Spring => random switch
                    {
                        < 70 => Weather.Clear,
                        < 90 => Weather.Rain,
                        var _ => Weather.Snow
                    },
                    Season.Summer => random switch
                    {
                        < 60 => Weather.Clear,
                        < 95 => Weather.Sunny,
                        var _ => Weather.Rain
                    },
                    Season.Fall => random switch
                    {
                        < 60 => Weather.Clear,
                        < 90 => Weather.Rain,
                        var _ => Weather.Snow
                    },
                    var _ => _currentWeather
                };
                break;
            }

            case Weather.Random:
            {
                _currentWeather = (Weather) Random.Shared.Next(0, 10);
                break;
            }

            case Weather.WeatherSeason:
            {
                GenerateNewWeather((Weather) _options.WorldOptions.WeatherSeason[(int) _currentSeason]);
                break;
            }

            default:
            {
                _currentWeather = targetWeather;
                break;
            }
        }
    }
}