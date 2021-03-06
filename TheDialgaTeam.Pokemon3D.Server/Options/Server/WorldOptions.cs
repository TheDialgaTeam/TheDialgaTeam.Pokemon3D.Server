namespace TheDialgaTeam.Pokemon3D.Server.Options.Server
{
    internal class WorldOptions
    {
        public int Season { get; set; } = -1;

        public int Weather { get; set; } = -1;

        public bool DoDayCycle { get; set; } = true;

        public int[] SeasonMonth { get; set; } = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

        public int[] WeatherSeason { get; set; } = { -1, -1, -1, -1 };
    }
}