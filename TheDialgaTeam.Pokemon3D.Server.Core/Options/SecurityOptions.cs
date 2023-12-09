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

using System.Security.Cryptography;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options;

public sealed class SecurityOptions
{
    public static readonly string[] SupportedPasswordHashingProvider = [
        Pbkdf2Options.ProviderName,
        Argon2Options.ProviderName
    ];

    public string PasswordHashingProvider { get; set; } = Pbkdf2Options.ProviderName;

    public Pbkdf2Options Pbkdf2 { get; set; } = new();
    
    public Argon2Options Argon2 { get; set; } = new();
}

public sealed class Pbkdf2Options
{
    public const string ProviderName = "Pbkdf2";

    public static readonly string[] SupportedHashingAlgorithm = [ 
        nameof(HashAlgorithmName.SHA1),
        nameof(HashAlgorithmName.SHA256),
        nameof(HashAlgorithmName.SHA384),
        nameof(HashAlgorithmName.SHA512),
        nameof(HashAlgorithmName.SHA3_256),
        nameof(HashAlgorithmName.SHA3_384),
        nameof(HashAlgorithmName.SHA3_512)
    ];

    public string HashingAlgorithm { get; set; } = nameof(HashAlgorithmName.SHA256);

    public int Iterations { get; set; } = 600000;
}

public sealed class Argon2Options
{
    public const string ProviderName = "Argon2";

    public int DegreeOfParallelism { get; set; } = 1;

    public int MemorySize { get; set; } = 7168;

    public int Iterations { get; set; } = 5;
}