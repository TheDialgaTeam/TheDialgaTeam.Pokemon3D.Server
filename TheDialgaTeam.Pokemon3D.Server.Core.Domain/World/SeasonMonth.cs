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

public class SeasonMonth
{
    public Tuple<Season, int>[] January => _january;
    public Tuple<Season, int>[] February => _february;
    public Tuple<Season, int>[] March => _march;
    public Tuple<Season, int>[] April => _april;
    public Tuple<Season, int>[] May => _may;
    public Tuple<Season, int>[] June => _june;
    public Tuple<Season, int>[] July => _july;
    public Tuple<Season, int>[] August => _august;
    public Tuple<Season, int>[] September => _september;
    public Tuple<Season, int>[] October => _october;
    public Tuple<Season, int>[] November => _november;
    public Tuple<Season, int>[] December => _december;

    private Tuple<Season, int>[] _january = [];
    private Tuple<Season, int>[] _february = [];
    private Tuple<Season, int>[] _march = [];
    private Tuple<Season, int>[] _april = [];
    private Tuple<Season, int>[] _may = [];
    private Tuple<Season, int>[] _june = [];
    private Tuple<Season, int>[] _july = [];
    private Tuple<Season, int>[] _august = [];
    private Tuple<Season, int>[] _september = [];
    private Tuple<Season, int>[] _october = [];
    private Tuple<Season, int>[] _november = [];
    private Tuple<Season, int>[] _december = [];

    private static void CheckAndSetIfValid(ref Tuple<Season, int>[] month, Tuple<Season, int>[] season)
    {
        month = season.All(tuple => tuple.Item1 != Season.SeasonMonth) ? season : throw new ArgumentException("SeasonMonth array must have a valid season.", nameof(season));
    }

    private static Season GetRandom(Tuple<Season, int>[] array)
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

    public void Update(IReadOnlyList<Tuple<Season, int>[]> seasons)
    {
        if (seasons.Count != 12) throw new ArgumentException("SeasonMonth array must have 12 entries.", nameof(seasons));

        CheckAndSetIfValid(ref _january, seasons[0]);
        CheckAndSetIfValid(ref _february, seasons[1]);
        CheckAndSetIfValid(ref _march, seasons[2]);
        CheckAndSetIfValid(ref _april, seasons[3]);
        CheckAndSetIfValid(ref _may, seasons[4]);
        CheckAndSetIfValid(ref _june, seasons[5]);
        CheckAndSetIfValid(ref _july, seasons[6]);
        CheckAndSetIfValid(ref _august, seasons[7]);
        CheckAndSetIfValid(ref _september, seasons[8]);
        CheckAndSetIfValid(ref _october, seasons[9]);
        CheckAndSetIfValid(ref _november, seasons[10]);
        CheckAndSetIfValid(ref _december, seasons[11]);
    }

    public Season GenerateNewSeason(int month)
    {
        return month switch
        {
            1 => GetRandom(_january),
            2 => GetRandom(_february),
            3 => GetRandom(_march),
            4 => GetRandom(_april),
            5 => GetRandom(_may),
            6 => GetRandom(_june),
            7 => GetRandom(_july),
            8 => GetRandom(_august),
            9 => GetRandom(_september),
            10 => GetRandom(_october),
            11 => GetRandom(_november),
            12 => GetRandom(_december),
            var _ => throw new ArgumentOutOfRangeException(nameof(month), month, null)
        };
    }
}