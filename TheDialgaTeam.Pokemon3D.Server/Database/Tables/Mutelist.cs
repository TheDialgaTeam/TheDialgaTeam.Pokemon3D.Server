using System;
using System.Text;

namespace TheDialgaTeam.Pokemon3D.Server.Database.Tables
{
    internal class Mutelist
    {
        public string Name { get; set; } = string.Empty;

        public string GameJoltId { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public DateTime StartTime { get; set; } = DateTime.Now;

        public int Duration { get; set; }

        public string GetDurationRemaining()
        {
            if (Duration == -1) return "Permanent";
            if (StartTime.AddSeconds(Duration) <= DateTime.Now) return string.Empty;

            var remainingTime = StartTime.AddSeconds(Duration) - DateTime.Now;

            switch (remainingTime.Days)
            {
                case > 1:
                    return $"{remainingTime.Days} Days";

                case 1:
                    return $"{remainingTime.Days} Day";

                default:
                {
                    var stringBuilder = new StringBuilder();

                    switch (remainingTime.Hours)
                    {
                        case 1:
                            stringBuilder.Append("1 Hour ");
                            break;

                        case > 1:
                            stringBuilder.Append(remainingTime.Hours);
                            stringBuilder.Append(" Hour ");
                            break;
                    }

                    switch (remainingTime.Minutes)
                    {
                        case > 1:
                            stringBuilder.Append(remainingTime.Minutes);
                            stringBuilder.Append(" Minutes ");
                            break;

                        default:
                            stringBuilder.Append(remainingTime.Minutes);
                            stringBuilder.Append(" Minute ");
                            break;
                    }

                    switch (remainingTime.Seconds)
                    {
                        case > 1:
                            stringBuilder.Append(remainingTime.Seconds);
                            stringBuilder.Append(" Seconds");
                            break;

                        default:
                            stringBuilder.Append(remainingTime.Seconds);
                            stringBuilder.Append(" Second");
                            break;
                    }

                    return stringBuilder.ToString();
                }
            }
        }
    }
}
