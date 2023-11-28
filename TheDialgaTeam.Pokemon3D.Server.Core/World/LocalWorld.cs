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

using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.World;

internal sealed partial class LocalWorld : ILocalWorld
{
    public Season CurrentSeason { get; private set; } = Season.Spring;

    public Weather CurrentWeather { get; private set; } = Weather.Clear;

    public DateTimeOffset CurrentTime => DateTimeOffset.UtcNow.Add(_targetOffset);
    
    private int WeekOfYear => (CurrentTime.DayOfYear - (CurrentTime.DayOfWeek - DayOfWeek.Monday)) / 7 + 1;

    private readonly ILogger? _logger;
    private readonly IPokemonServerOptions _options;

    private readonly bool _isGlobalWorld = true;
    
    private Season _targetSeason;
    private Weather _targetWeather;
    private TimeSpan _targetOffset;

    private DateTimeOffset _lastWorldUpdate = DateTimeOffset.MinValue;

    private readonly Timer _timer;

    public LocalWorld(ILogger<LocalWorld>? logger, IPokemonServerOptions options)
    {
        _logger = logger;
        _options = options;

        _targetSeason = _options.WorldOptions.Season;
        _targetWeather = _options.WorldOptions.Weather;
        _targetOffset = _options.WorldOptions.TimeOffset;

        _timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    public LocalWorld(IPokemonServerOptions options, Season season, Weather weather, TimeSpan offset) : this(null, options)
    {
        _isGlobalWorld = false;
        
        _targetSeason = season;
        _targetWeather = weather;
        _targetOffset = offset;
    }

    public WorldDataPacket GetRawPacket()
    {
        return new WorldDataPacket(CurrentSeason, CurrentWeather, TimeOnly.FromDateTime(CurrentTime.DateTime));
    }

    private void TimerCallback(object? state)
    {
        if (_isGlobalWorld)
        {
            _targetSeason = _options.WorldOptions.Season;
            _targetWeather = _options.WorldOptions.Weather;
            _targetOffset = _options.WorldOptions.TimeOffset;
        }
        
        var currentTime = CurrentTime;

        if (_lastWorldUpdate.Day != currentTime.Day)
        {
            GenerateNewSeason(_targetSeason);
        }

        if (_lastWorldUpdate.Hour != currentTime.Hour)
        {
            GenerateNewWeather(_targetWeather);

            if (_isGlobalWorld)
            {
                PrintCurrentWorld(Enum.GetName(CurrentSeason) ?? string.Empty, Enum.GetName(CurrentWeather) ?? string.Empty, CurrentTime.DateTime);
            }
        }

        _lastWorldUpdate = currentTime;
    }

    private void GenerateNewSeason(Season targetSeason)
    {
        switch (targetSeason)
        {
            case Season.Default:
            {
                CurrentSeason = (WeekOfYear % 4) switch
                {
                    0 => Season.Fall,
                    1 => Season.Winter,
                    2 => Season.Spring,
                    3 => Season.Summer,
                    var _ => CurrentSeason
                };
                break;
            }

            case Season.Random:
            {
                CurrentSeason = (Season) Random.Shared.Next(0, 4);
                break;
            }

            case Season.SeasonMonth:
            {
                GenerateNewSeason((Season) _options.WorldOptions.SeasonMonth[CurrentTime.Month]);
                break;
            }

            default:
            {
                CurrentSeason = targetSeason;
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

                CurrentWeather = CurrentSeason switch
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
                        < 95 => CurrentTime.Hour switch
                        {
                            < 9 => Weather.Clear,
                            < 19 => Weather.Sunny,
                            var _ => Weather.Clear
                        },
                        var _ => Weather.Rain
                    },
                    Season.Fall => random switch
                    {
                        < 60 => Weather.Clear,
                        < 90 => Weather.Rain,
                        var _ => Weather.Snow
                    },
                    var _ => CurrentWeather
                };
                break;
            }

            case Weather.Random:
            {
                CurrentWeather = (Weather) Random.Shared.Next(0, 10);
                break;
            }

            case Weather.WeatherSeason:
            {
                GenerateNewWeather((Weather) _options.WorldOptions.WeatherSeason[(int) CurrentSeason]);
                break;
            }

            default:
            {
                CurrentWeather = targetWeather;
                break;
            }
        }
    }

    [LoggerMessage(LogLevel.Information, "[World] Current Season: {season} | Current Weather: {weather} | Current Time: {time}")]
    private partial void PrintCurrentWorld(string season, string weather, DateTime time);

    public void Dispose()
    {
        _timer.Dispose();
    }
}