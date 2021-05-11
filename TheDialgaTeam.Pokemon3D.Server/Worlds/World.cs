using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Core.Logger.Formatter;
using TheDialgaTeam.Pokemon3D.Server.Options.Server;
using TheDialgaTeam.Pokemon3D.Server.Packages;
using TheDialgaTeam.Pokemon3D.Server.Players;
using TheDialgaTeam.Pokemon3D.Server.Serilog;

namespace TheDialgaTeam.Pokemon3D.Server.Worlds
{
    internal class World : IDisposable
    {
        private static readonly Random RandomNumberGenerator = new();

        private readonly Logger _logger;
        private readonly IOptionsMonitor<WorldOptions> _optionsMonitor;
        private readonly PlayerCollection _playerCollection;
        private readonly Timer _worldUpdateTimer;

        private DateTime _lastWorldUpdate = DateTime.Now;

        public Season Season { get; set; } = Season.Summer;

        public Weather Weather { get; set; } = Weather.Clear;

        public World(Logger logger, IOptionsMonitor<WorldOptions> optionsMonitor, PlayerCollection playerCollection)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _playerCollection = playerCollection;
            _worldUpdateTimer = new Timer(WorldUpdateTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        private static Season GenerateNewSeason(DateTime targetDateTime, int seasonOption)
        {
            switch (seasonOption)
            {
                case 0:
                    return Season.Fall;

                case 1:
                    return Season.Winter;

                case 2:
                    return Season.Spring;

                case 3:
                    return Season.Summer;

                case -2:
                    return GenerateNewSeason(targetDateTime, RandomNumberGenerator.Next(0, 4));

                default:
                {
                    var weekOfYear = (int) ((targetDateTime.DayOfYear - (targetDateTime.DayOfWeek - DayOfWeek.Monday)) / 7.0 + 1.0);

                    return (weekOfYear % 4) switch
                    {
                        0 => Season.Fall,
                        1 => Season.Winter,
                        2 => Season.Spring,
                        3 => Season.Summer,
                        var _ => Season.Summer
                    };
                }
            }
        }

        private static Weather GenerateNewWeather(Season targetSeason, int weatherOption)
        {
            switch (weatherOption)
            {
                case 0:
                    return Weather.Clear;

                case 1:
                    return Weather.Rain;

                case 2:
                    return Weather.Snow;

                case 3:
                    return Weather.Underwater;

                case 4:
                    return Weather.Sunny;

                case 5:
                    return Weather.Fog;

                case 6:
                    return Weather.Thunderstorm;

                case 7:
                    return Weather.Sandstorm;

                case 8:
                    return Weather.Ash;

                case 9:
                    return Weather.Blizzard;

                case -2:
                    return GenerateNewWeather(targetSeason, RandomNumberGenerator.Next(0, 10));

                default:
                {
                    var random = RandomNumberGenerator.Next(0, 100);

                    switch (targetSeason)
                    {
                        case Season.Fall:
                            if (random < 5) return Weather.Snow;
                            return random < 80 ? Weather.Rain : Weather.Clear;

                        case Season.Winter:
                            if (random < 20) return Weather.Rain;
                            return random < 50 ? Weather.Clear : Weather.Snow;

                        case Season.Spring:
                            if (random < 5) return Weather.Snow;
                            return random < 40 ? Weather.Rain : Weather.Clear;

                        case Season.Summer:
                            return random < 10 ? Weather.Rain : Weather.Clear;

                        default:
                            return Weather.Clear;
                    }
                }
            }
        }

        public void StartWorld()
        {
            _logger.LogInformation("[World] Starting World", true);
            _worldUpdateTimer.Change(0, 1000);
            _logger.LogInformation($"[World] {AnsiEscapeCodeConstants.GreenForegroundColor}World Started{AnsiEscapeCodeConstants.DefaultColor}", true);

            GenerateNewSeasonAndWeather(DateTime.Now);
        }

        public void StopWorld()
        {
            _logger.LogInformation("[World] Stopping World", true);
            _worldUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _logger.LogInformation($"[World] {AnsiEscapeCodeConstants.GreenForegroundColor}World Stopped{AnsiEscapeCodeConstants.DefaultColor}", true);
        }

        public override string ToString()
        {
            return $"Current Season: {Season} | Current Weather: {Weather} | Current Time: {DateTime.Now}";
        }

        private void GenerateNewSeasonAndWeather(DateTime targetDateTime)
        {
            _lastWorldUpdate = targetDateTime;

            if (_optionsMonitor.CurrentValue.DoDayCycle)
            {
                Season = GenerateNewSeason(targetDateTime);
                Weather = GenerateNewWeather(Season);
            }

            _logger.LogInformation("[World] {World:l}", true, ToString());
        }

        private Season GenerateNewSeason(DateTime targetDateTime)
        {
            var worldOptions = _optionsMonitor.CurrentValue;

            try
            {
                return GenerateNewSeason(targetDateTime, worldOptions.Season == -3 ? worldOptions.SeasonMonth[targetDateTime.Month - 1] : worldOptions.Season);
            }
            catch (IndexOutOfRangeException)
            {
                _logger.LogWarning("[World] SeasonMonth option is missing a parameter for the target month", true);
                return GenerateNewSeason(targetDateTime, -1);
            }
        }

        private Weather GenerateNewWeather(Season targetSeason)
        {
            var worldOptions = _optionsMonitor.CurrentValue;

            try
            {
                return GenerateNewWeather(targetSeason, worldOptions.Weather == -3 ? worldOptions.WeatherSeason[(int) targetSeason] : worldOptions.Weather);
            }
            catch (IndexOutOfRangeException)
            {
                _logger.LogWarning("[World] WeatherSeason option is missing a parameter for the target season", true);
                return GenerateNewWeather(targetSeason, -1);
            }
        }

        private void WorldUpdateTimerCallback(object? state)
        {
            var currentTime = DateTime.Now;

            if (_lastWorldUpdate.Hour != currentTime.Hour) GenerateNewSeasonAndWeather(currentTime);

            _playerCollection.SendToAllPlayers(new Package(PackageType.WorldData, new List<string>
            {
                ((int) Season).ToString(),
                ((int) Weather).ToString(),
                $"{currentTime.Hour},{currentTime.Minute},{currentTime.Second}"
            }));
        }

        public void Dispose()
        {
            _worldUpdateTimer.Dispose();
        }
    }
}