using System.Globalization;

namespace TheDialgaTeam.Pokemon3D.Server.Players
{
    internal readonly struct Position
    {
        public double X { get; init; }

        public double Y { get; init; }

        public double Z { get; init; }

        public Position(string position, string decimalSeparator)
        {
            var positionString = position.Split('|');
            var numberFormatInfo = new NumberFormatInfo { NumberDecimalSeparator = decimalSeparator };

            X = double.Parse(positionString[0], NumberStyles.Number, numberFormatInfo);
            Y = double.Parse(positionString[1], NumberStyles.Number, numberFormatInfo);
            Z = double.Parse(positionString[2], NumberStyles.Number, numberFormatInfo);
        }

        public override string ToString()
        {
            return $"{X}|{Y}|{Z}";
        }
    }
}