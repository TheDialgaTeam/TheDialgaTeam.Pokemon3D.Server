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

internal sealed class ServerOptionsValidation : IValidateOptions<ServerOptions>
{
    public ValidateOptionsResult Validate(string? name, ServerOptions options)
    {
        if (string.IsNullOrEmpty(options.ServerName))
        {
            return ValidateOptionsResult.Fail($"[Server:{nameof(options.ServerName)}] Server name cannot be empty.");
        }
        
        if (options.MaxPlayers <= 0)
        {
            return ValidateOptionsResult.Fail($"[Server:{nameof(options.MaxPlayers)}] Max player limit must be more than 0.");
        }
        
        if (options.NoPingKickTime.TotalSeconds < 10)
        {
            return ValidateOptionsResult.Fail($"[Server:{nameof(options.NoPingKickTime)}] NoPingKickTime must be at least 10 seconds.");
        }
        
        return ValidateOptionsResult.Success;
    }
}