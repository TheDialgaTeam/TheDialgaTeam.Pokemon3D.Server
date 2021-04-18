using System;
using System.Timers;

namespace TheDialgaTeam.Pokemon3D.Server.Worlds
{
    internal class World : IDisposable
    {
        private static readonly Random RandomNumberGenerator = new();

        private readonly Timer _worldUpdateTimer;
        private DateTime? _lastWorldUpdate;

        public Season Season { get; set; } = Season.Summer;

        public Weather Weather { get; set; } = Weather.Clear;

        public World()
        {
            _worldUpdateTimer = new Timer { AutoReset = true, Interval = 1000 };
            _worldUpdateTimer.Elapsed += WorldUpdateTimerOnElapsed;
            _worldUpdateTimer.Start();
        }

        public static Season GenerateNewSeason(DateTime targetDateTime)
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

        public static Weather GenerateNewWeather(Season targetSeason)
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

        public override string ToString()
        {
            return $"Current Season: {Season} | Current Weather: {Weather} | Current Time: {DateTime.Now}";
        }

        private void WorldUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var lastWorldUpdate = _lastWorldUpdate;

            if (lastWorldUpdate == null || e.SignalTime >= lastWorldUpdate.Value.AddHours(1))
            {
                _lastWorldUpdate = e.SignalTime;

                Season = GenerateNewSeason(e.SignalTime);
                Weather = GenerateNewWeather(Season);
            }
        }

        public void Dispose()
        {
            _worldUpdateTimer.Dispose();
        }
    }
}