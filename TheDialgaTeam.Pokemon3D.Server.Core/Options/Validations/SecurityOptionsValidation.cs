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

using Microsoft.Extensions.Options;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options.Validations;

internal sealed class SecurityOptionsValidation : IValidateOptions<SecurityOptions>
{
    public ValidateOptionsResult Validate(string? name, SecurityOptions options)
    {
        var failureMessages = new List<string>();
        
        if (!SecurityOptions.SupportedPasswordHashingProvider.Contains(options.PasswordHashingProvider))
        {
            failureMessages.Add($"[Server:Security:{nameof(options.PasswordHashingProvider)}] Selected provider is not supported.");
        }
        else
        {
            if (options.PasswordHashingProvider == Pbkdf2Options.ProviderName)
            {
                if (!Pbkdf2Options.SupportedHashingAlgorithm.Contains(options.Pbkdf2.HashingAlgorithm))
                {
                    failureMessages.Add($"[Server:Security:{nameof(options.Pbkdf2)}:{nameof(options.Pbkdf2.HashingAlgorithm)}] Selected hashing algorithm is not supported.");
                }
            }
        }

        return failureMessages.Count > 0 ? ValidateOptionsResult.Fail(failureMessages) : ValidateOptionsResult.Success;
    }
}