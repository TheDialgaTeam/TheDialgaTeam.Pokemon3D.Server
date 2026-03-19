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
    public Tuple<Season, int>[] January { get; private set; }
    public Tuple<Season, int>[] February { get; private set; }
    public Tuple<Season, int>[] March { get; private set; }
    public Tuple<Season, int>[] April { get; private set; }
    public Tuple<Season, int>[] May { get; private set; }
    public Tuple<Season, int>[] June { get; private set; }
    public Tuple<Season, int>[] July { get; private set; }
    public Tuple<Season, int>[] August { get; private set; }
    public Tuple<Season, int>[] September { get; private set; }
    public Tuple<Season, int>[] October { get; private set; }
    public Tuple<Season, int>[] November { get; private set; }
    public Tuple<Season, int>[] December { get; private set; }

    public SeasonMonth(IReadOnlyList<Tuple<Season, int>[]> seasons)
    {
        if (seasons.Count != 12) throw new ArgumentException("Seasons must have 12 seasons", nameof(seasons));
        
        January = seasons[0];
        February = seasons[1];
        March = seasons[2];
        April = seasons[3];
        May = seasons[4];
        June = seasons[5];
        July = seasons[6];
        August = seasons[7];
        September = seasons[8];
        October = seasons[9];
        November = seasons[10];
        December = seasons[11];
    }
}