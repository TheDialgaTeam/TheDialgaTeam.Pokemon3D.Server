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

using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using TheDialgaTeam.Pokemon3D.Server.Core.Options;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Security.PasswordHashing.Providers;

internal sealed class Argon2PasswordHashingProvider : IPasswordHashingProvider
{
    private const int SaltLength = 16;
    private const int HashSize = 32;
    
    private readonly Argon2Options _options;

    public Argon2PasswordHashingProvider(Argon2Options options)
    {
        _options = options;
    }
    
    public bool ComparePassword(string password, string passwordHash)
    {
        return GeneratePasswordHash(password).Equals(passwordHash);
    }

    public string GeneratePasswordHash(string password)
    {
        var salt = new byte[SaltLength];
        RandomNumberGenerator.Fill(salt);

        var argon2Id = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            DegreeOfParallelism = _options.DegreeOfParallelism,
            MemorySize = _options.MemorySize,
            Iterations = _options.Iterations,
            Salt = salt.ToArray()
        };
        
        var hash = argon2Id.GetBytes(HashSize);

        const int combinedHashLength = SaltLength + HashSize;
        var combinedHash = ArrayPool<byte>.Shared.Rent(combinedHashLength);

        try
        {
            salt.CopyTo(combinedHash, 0);
            hash.CopyTo(combinedHash, SaltLength);

            return Convert.ToBase64String(combinedHash.AsSpan(0, combinedHashLength));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(combinedHash);
        }
    }
}