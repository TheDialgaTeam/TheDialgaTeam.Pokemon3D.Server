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

using Microsoft.Data.Sqlite;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options;

public sealed class DatabaseOptions
{
    public static readonly string[] SupportedProviders = [nameof(Sqlite)];

    public string DatabaseProvider { get; set; } = nameof(Sqlite);

    public SqliteOptions Sqlite { get; set; } = new();
}

public sealed class SqliteOptions
{
    public string DataSource { get; set; } = "data.db";

    public SqliteOpenMode Mode { get; set; } = SqliteOpenMode.ReadWriteCreate;

    public SqliteCacheMode Cache { get; set; } = SqliteCacheMode.Default;

    public string Password { get; set; } = string.Empty;
    
    public bool? ForeignKeys { get; set; }
    
    public bool RecursiveTriggers { get; set; }

    public int DefaultTimeout { get; set; } = 30;

    public bool Pooling { get; set; } = true;
}