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
using TheDialgaTeam.Pokemon3D.Server.Core.Options;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Security.PasswordHashing.Providers;

internal sealed class Pbkdf2PasswordHashingProvider : IPasswordHashingProvider
{
    private const int SaltLength = 16;
    
    private readonly Pbkdf2Options _options;
    
    public Pbkdf2PasswordHashingProvider(Pbkdf2Options options)
    {
        _options = options;
    }

    public bool ComparePassword(string password, string passwordHash)
    {
        return GeneratePasswordHash(password).Equals(passwordHash);
    }

    public string GeneratePasswordHash(string password)
    {
        // Password hash consist of 16 bytes salt, N bytes hash (based on hash size)
        
        var hashLength = _options.HashingAlgorithm switch
        {
            nameof(HashAlgorithmName.SHA1) => SHA1.HashSizeInBytes,
            nameof(HashAlgorithmName.SHA256) => SHA256.HashSizeInBytes,
            nameof(HashAlgorithmName.SHA384) => SHA384.HashSizeInBytes,
            nameof(HashAlgorithmName.SHA512) => SHA512.HashSizeInBytes,
            nameof(HashAlgorithmName.SHA3_256) => SHA3_256.HashSizeInBytes,
            nameof(HashAlgorithmName.SHA3_384) => SHA3_384.HashSizeInBytes,
            nameof(HashAlgorithmName.SHA3_512) => SHA3_512.HashSizeInBytes,
            var _ => throw new NotSupportedException()
        };

        var combinedHashLength = SaltLength + hashLength;
        var hash = ArrayPool<byte>.Shared.Rent(combinedHashLength);

        try
        {
            RandomNumberGenerator.Fill(hash.AsSpan(0, 16));
            Rfc2898DeriveBytes.Pbkdf2(password, hash.AsSpan(0, SaltLength), hash.AsSpan(SaltLength, hashLength), _options.Iterations, Enum.Parse<HashAlgorithmName>(_options.HashingAlgorithm));
            return Convert.ToBase64String(hash.AsSpan(0, combinedHashLength));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(hash);
        }
    }
}