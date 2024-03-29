﻿// Pokemon 3D Server Client
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

using Mediator;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packets;
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.World.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.World;

internal sealed class LocalWorld : ILocalWorld
{
    public Season CurrentSeason { get; private set; }

    public Weather CurrentWeather { get; private set; }

    public DateTime CurrentTime { get; private set; }
    
    public bool DoDayCycle { get; set; }
    
    public Season TargetSeason { get; set; }
    
    public Weather TargetWeather { get; set; }
    
    public TimeSpan TargetOffset { get; set; }

    public IPlayer? Player { get; }

    private int WeekOfYear => (CurrentTime.DayOfYear - (CurrentTime.DayOfWeek - DayOfWeek.Monday)) / 7 + 1;

    private readonly IMediator _mediator;
    private readonly IPokemonServerOptions _options;
    private readonly ILocalWorld? _world;

    private readonly Timer _timer;

    public LocalWorld(IMediator mediator, IPokemonServerOptions options, ILocalWorld? world = null, IPlayer? player = null)
    {
        _mediator = mediator;
        _options = options;
        _world = world;
        Player = player;
        _timer = new Timer(TimerCallback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public bool StartWorld()
    {
        return _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }
    
    public bool StopWorld()
    {
        return _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public WorldDataPacket GetWorldDataPacket()
    {
        return new WorldDataPacket(CurrentSeason, CurrentWeather, TimeOnly.FromDateTime(CurrentTime));
    }

    private void TimerCallback(object? state)
    {
        if (_world is null)
        {
            DoDayCycle = _options.WorldOptions.DoDayCycle;
            TargetSeason = _options.WorldOptions.Season;
            TargetWeather = _options.WorldOptions.Weather;
            TargetOffset = TimeSpan.FromMinutes(_options.WorldOptions.TimeOffset);
        }
        else
        {
            DoDayCycle = Player?.PlayerProfile?.LocalWorld.DoDayCycle ?? _options.WorldOptions.DoDayCycle;
            TargetSeason = Player?.PlayerProfile?.LocalWorld.Season ?? _options.WorldOptions.Season;
            TargetWeather = Player?.PlayerProfile?.LocalWorld.Weather ?? _options.WorldOptions.Weather;
            TargetOffset = TimeSpan.FromMinutes(Player?.PlayerProfile?.LocalWorld.TimeOffset ?? _options.WorldOptions.TimeOffset);
        }

        if (!DoDayCycle)
        {
            CurrentSeason = TargetSeason > Season.Default ? TargetSeason : Season.Winter;
            CurrentWeather = TargetWeather > Weather.Default ? TargetWeather : Weather.Clear;
            CurrentTime = DateTime.Today.AddHours(12);
            
            Task.Run(() => _mediator.Publish(new LocalWorldUpdate(this)).AsTask());
            return;
        }

        var previousDateTime = CurrentTime;
        CurrentTime = DateTimeOffset.UtcNow.Add(TargetOffset).DateTime;
        var generatedNewWorld = false;

        if (CurrentTime.Date != previousDateTime.Date)
        {
            GenerateNewSeason(TargetSeason);
            generatedNewWorld = true;
        }

        if (CurrentTime.Date != previousDateTime.Date || CurrentTime.Hour != previousDateTime.Hour)
        {
            GenerateNewWeather(TargetWeather);
            generatedNewWorld = true;
        }

        if (!generatedNewWorld) return;

        Task.Run(() => _mediator.Publish(new LocalWorldUpdate(this)).AsTask());
    }

    private void GenerateNewSeason(Season targetSeason)
    {
        switch (targetSeason)
        {
            case Season.Default:
            {
                if (_world != null)
                {
                    CurrentSeason = _world.CurrentSeason;
                    return;
                }
                
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
                if (_world != null)
                {
                    CurrentWeather = _world.CurrentWeather;
                    return;
                }
                
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

    public void Dispose()
    {
        _timer.Dispose();
    }
}