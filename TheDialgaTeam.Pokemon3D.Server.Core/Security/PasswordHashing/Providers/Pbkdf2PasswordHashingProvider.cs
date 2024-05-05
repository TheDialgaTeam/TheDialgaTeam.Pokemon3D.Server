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
using TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Security.PasswordHashing.Providers;

internal sealed class Pbkdf2PasswordHashingProvider(Pbkdf2Options options) : IPasswordHashingProvider
{
    private const int SaltLength = 16;

    public bool ComparePassword(string password, string passwordHash)
    {
        var providerId = passwordHash.AsSpan(1).SplitNext('$', out var next);
        if (providerId != "pbkdf2") return false;
        
        var hashAlgorithm = new HashAlgorithmName(next.SplitNext('$', out next).ToString());
        int hashLength;
        
        if (hashAlgorithm == HashAlgorithmName.SHA1)
        {
            hashLength = SHA1.HashSizeInBytes;
        }
        else if (hashAlgorithm == HashAlgorithmName.SHA256)
        {
            hashLength = SHA256.HashSizeInBytes;
        }
        else if (hashAlgorithm == HashAlgorithmName.SHA384)
        {
            hashLength = SHA384.HashSizeInBytes;
        }
        else if (hashAlgorithm == HashAlgorithmName.SHA512)
        {
            hashLength = SHA512.HashSizeInBytes;
        }
        else if (hashAlgorithm == HashAlgorithmName.SHA3_256)
        {
            hashLength = SHA3_256.HashSizeInBytes;
        }
        else if (hashAlgorithm == HashAlgorithmName.SHA3_384)
        {
            hashLength = SHA3_384.HashSizeInBytes;
        }
        else if (hashAlgorithm == HashAlgorithmName.SHA3_512)
        {
            hashLength = SHA3_512.HashSizeInBytes;
        }
        else
        {
            throw new NotSupportedException();
        }

        if (!int.TryParse(next.SplitNext('$', out next), out var iterations)) return false;
        
        var combinedHashLength = SaltLength + hashLength;
        var hash = ArrayPool<byte>.Shared.Rent(combinedHashLength);

        if (!Convert.TryFromBase64String(next.SplitNext('$', out next).ToString(), hash.AsSpan(0, SaltLength), out var _)) return false;
        
        try
        {
            Rfc2898DeriveBytes.Pbkdf2(password, hash.AsSpan(0, SaltLength), hash.AsSpan(SaltLength, hashLength), iterations, hashAlgorithm);
            return $"$pbkdf2${hashAlgorithm.Name}${options.Iterations}${Convert.ToBase64String(hash.AsSpan(0, SaltLength))}${Convert.ToBase64String(hash.AsSpan(SaltLength, hashLength))}" == passwordHash;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(hash);
        }
    }

    public string GeneratePasswordHash(string password)
    {
        // Encoding Format: $pbkdf2$<HashName>$<Iterations>$<Salt>$<Hash>

        var hashAlgorithm = new HashAlgorithmName(options.HashingAlgorithm);
        int hashLength;

        if (hashAlgorithm == HashAlgorithmName.SHA1)
        {
            hashLength = SHA1.HashSizeInBytes;
        }
        else if (hashAlgorithm == HashAlgorithmName.SHA256)
        {
            hashLength = SHA256.HashSizeInBytes;
        }
        else if (hashAlgorithm == HashAlgorithmName.SHA384)
        {
            hashLength = SHA384.HashSizeInBytes;
        }
        else if (hashAlgorithm == HashAlgorithmName.SHA512)
        {
            hashLength = SHA512.HashSizeInBytes;
        }
        else if (hashAlgorithm == HashAlgorithmName.SHA3_256)
        {
            hashLength = SHA3_256.HashSizeInBytes;
        }
        else if (hashAlgorithm == HashAlgorithmName.SHA3_384)
        {
            hashLength = SHA3_384.HashSizeInBytes;
        }
        else if (hashAlgorithm == HashAlgorithmName.SHA3_512)
        {
            hashLength = SHA3_512.HashSizeInBytes;
        }
        else
        {
            throw new NotSupportedException();
        }

        var combinedHashLength = SaltLength + hashLength;
        var hash = ArrayPool<byte>.Shared.Rent(combinedHashLength);

        try
        {
            RandomNumberGenerator.Fill(hash.AsSpan(0, 16));
            Rfc2898DeriveBytes.Pbkdf2(password, hash.AsSpan(0, SaltLength), hash.AsSpan(SaltLength, hashLength), options.Iterations, hashAlgorithm);
            return $"$pbkdf2${hashAlgorithm.Name}${options.Iterations}${Convert.ToBase64String(hash.AsSpan(0, SaltLength))}${Convert.ToBase64String(hash.AsSpan(SaltLength, hashLength))}";
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(hash);
        }
    }
}