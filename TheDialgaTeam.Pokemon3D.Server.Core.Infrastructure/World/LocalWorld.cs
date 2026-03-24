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

using System.Reactive.Linq;
using System.Reactive.Subjects;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.World;
using TheDialgaTeam.Pokemon3D.Server.Core.Domain.World;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.World;

public class LocalWorld : ILocalWorld, IAsyncDisposable
{
    public Season CurrentSeason { get; private set; } = Season.Winter;
    public Weather CurrentWeather { get; private set; } = Weather.Clear;
    public DateTime CurrentLocalTime { get; private set; } = DateTime.Now;

    public bool DoDayCycle { get; private set; } = true;
    public Season TargetSeason { get; private set; } = Season.Default;
    public Weather TargetWeather { get; private set; } = Weather.Default;
    public TimeSpan TargetOffset { get; private set; } = DateTimeOffset.Now.Offset;
    public SeasonMonthValue[][] TargetSeasonMonth { get; private set; } = [];
    public WeatherSeasonValue[][] TargetWeatherSeason { get; private set; } = [];

    public IObservable<ILocalWorld> ObserveLocalWorld => _localWorldSubject.AsObservable();
    
    private int WeekOfYear => (CurrentLocalTime.DayOfYear - 1) / 7 + 1;

    private readonly Timer _timer;
    private DateTimeOffset _lastUpdate = DateTimeOffset.MinValue;

    private readonly BehaviorSubject<ILocalWorld> _localWorldSubject;

    public LocalWorld(GameModeOverrideOptions options)
    {
        _timer = new Timer(TimerTick, this, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _localWorldSubject = new BehaviorSubject<ILocalWorld>(this);
        UpdateWorld(options);
    }

    public void StartWorld()
    {
        _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    public void StopWorld()
    {
        _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public void UpdateWorld(GameModeOverrideOptions options)
    {
        DoDayCycle = options.DoDayCycle.GetValueOrDefault(DoDayCycle);
        TargetSeason = options.Season.GetValueOrDefault(TargetSeason);
        TargetWeather = options.Weather.GetValueOrDefault(TargetWeather);
        TargetOffset = options.TimeOffset.GetValueOrDefault(TargetOffset);
        TargetSeasonMonth = options.SeasonMonth ?? TargetSeasonMonth;
        TargetWeatherSeason = options.WeatherSeason ?? TargetWeatherSeason;
    }

    private void TimerTick(object? state)
    {
        var currentServerTime = DateTimeOffset.Now;
        DateTime newLocalTime;
        Season newSeason;
        Weather newWeather;
        bool hasChanges;
        
        if (!DoDayCycle)
        {
            newLocalTime = new DateTime(currentServerTime.Year, currentServerTime.Month, currentServerTime.Day, 12, 0, 0);
            newSeason = Season.Winter;
            newWeather = Weather.Clear;
            hasChanges = newLocalTime != CurrentLocalTime || newSeason != CurrentSeason || newWeather != CurrentWeather;
            
            CurrentLocalTime = newLocalTime;
            CurrentSeason = newSeason;
            CurrentWeather = newWeather;
            
            _lastUpdate = currentServerTime;
        }
        else
        {
            var currentTime = currentServerTime.ToOffset(TargetOffset);
            
            newLocalTime = currentTime.LocalDateTime;
            newSeason = currentTime.DayOfYear != _lastUpdate.DayOfYear ? GenerateNewSeason(TargetSeason) : CurrentSeason;
            newWeather = currentTime.Hour != _lastUpdate.Hour ? GenerateNewWeather(TargetWeather) : CurrentWeather;
            hasChanges = newLocalTime != CurrentLocalTime || newSeason != CurrentSeason || newWeather != CurrentWeather;
            
            CurrentLocalTime = currentTime.LocalDateTime;
            CurrentSeason = newSeason;
            CurrentWeather = newWeather;
            
            _lastUpdate = currentTime;
        }

        if (hasChanges)
        {
            _localWorldSubject.OnNext(this);
        }
    }
    
    private Season GenerateNewSeason(Season targetSeason)
    {
        return targetSeason switch
        {
            Season.Default => (WeekOfYear % 4) switch
            {
                0 => Season.Fall,
                1 => Season.Winter,
                2 => Season.Spring,
                3 => Season.Summer,
                var _ => CurrentSeason
            },
            Season.Random => (Season) Random.Shared.Next(0, 4),
            Season.SeasonMonth => GenerateNewSeason(GenerateSeasonFromSeasonMonth()),
            var _ => targetSeason
        };
    }
    
    private Weather GenerateNewWeather(Weather targetWeather)
    {
        return targetWeather switch
        {
            Weather.Default => CurrentSeason switch
            {
                Season.Winter => Random.Shared.Next(0, 100) switch
                {
                    /*
                     * During Winter the odds of the weather are as follows:
                     * Clear - 25
                     * Rain - 5
                     * Thunderstorm - 2
                     * Sunny - 10
                     * Snow - 30
                     * Blizzard - 15
                     * Fog - 8
                     * Mist - 5
                     */
                    < 25 => Weather.Clear,
                    < 30 => Weather.Rain,
                    < 32 => Weather.Thunderstorm,
                    < 42 => CurrentLocalTime.Hour switch
                    {
                        < 8 => Weather.Clear,
                        < 16 => Weather.Sunny,
                        var _ => Weather.Clear
                    },
                    < 72 => Weather.Snow,
                    < 87 => Weather.Blizzard,
                    < 95 => Weather.Fog,
                    var _ => Weather.Mist
                },
                Season.Spring => Random.Shared.Next(0, 100) switch
                {
                    /*
                     * During Spring the odds of the weather are as follows:
                     * Clear - 20
                     * Rain - 30
                     * Thunderstorm - 10
                     * Sunny - 20
                     * Snow - 2
                     * Fog - 10
                     * Mist - 8
                     */
                    < 20 => Weather.Clear,
                    < 50 => Weather.Rain,
                    < 60 => Weather.Thunderstorm,
                    < 80 => CurrentLocalTime.Hour switch
                    {
                        < 6 => Weather.Clear,
                        < 18 => Weather.Sunny,
                        var _ => Weather.Clear
                    },
                    < 82 => Weather.Snow,
                    < 92 => Weather.Fog,
                    var _ => Weather.Mist
                },
                Season.Summer => Random.Shared.Next(0, 100) switch
                {
                    /*
                     * During Summer the odds of the weather are as follows:
                     * Clear - 25
                     * Rain - 15
                     * Thunderstorm - 15
                     * Sunny - 40
                     * Fog - 3
                     * Mist - 2
                     */
                    < 25 => Weather.Clear,
                    < 40 => Weather.Rain,
                    < 55 => Weather.Thunderstorm,
                    < 95 => CurrentLocalTime.Hour switch
                    {
                        < 5 => Weather.Clear,
                        < 20 => Weather.Sunny,
                        var _ => Weather.Clear
                    },
                    < 98 => Weather.Fog,
                    var _ => Weather.Mist
                },
                Season.Fall => Random.Shared.Next(0, 100) switch
                {
                    /*
                     * During Fall the odds of the weather are as follows:
                     * Clear - 25
                     * Rain - 25
                     * Thunderstorm - 5
                     * Sunny - 20
                     * Snow - 5
                     * Fog - 10
                     * Mist - 10
                     */
                    < 25 => Weather.Clear,
                    < 50 => Weather.Rain,
                    < 55 => Weather.Thunderstorm,
                    < 75 => CurrentLocalTime.Hour switch
                    {
                        < 7 => Weather.Clear,
                        < 17 => Weather.Sunny,
                        var _ => Weather.Clear
                    },
                    < 80 => Weather.Snow,
                    < 90 => Weather.Fog,
                    var _ => Weather.Mist
                },
                var _ => CurrentWeather
            },
            Weather.Random => (Weather) Random.Shared.Next(0, 11),
            Weather.WeatherSeason => GenerateNewWeather(generateWeatherFromWeatherSeason()),
            var _ => targetWeather
        };
    }

    private Season GenerateSeasonFromSeasonMonth()
    {
        var targetSeasonMonth = TargetSeasonMonth[CurrentLocalTime.Month];
        var sum = targetSeasonMonth.Sum(value => value.Chance);
        var target = Random.Shared.Next(0, sum);
        var current = 0;
        
        foreach (var value in targetSeasonMonth)
        {
            current += value.Chance;

            if (target < current)
            {
                return value.Season;
            }
        }

        return targetSeasonMonth[^1].Season;
    }
    
    private Weather generateWeatherFromWeatherSeason()
    {
        var targetWeatherSeason = TargetWeatherSeason[(int) CurrentSeason];
        var sum = targetWeatherSeason.Sum(value => value.Chance);
        var target = Random.Shared.Next(0, sum);
        var current = 0;
        
        foreach (var value in targetWeatherSeason)
        {
            current += value.Chance;

            if (target < current)
            {
                return value.Weather;
            }
        }

        return targetWeatherSeason[^1].Weather;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _timer.Dispose();
        _localWorldSubject.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await _timer.DisposeAsync().ConfigureAwait(false);
        _localWorldSubject.Dispose();
    }
}