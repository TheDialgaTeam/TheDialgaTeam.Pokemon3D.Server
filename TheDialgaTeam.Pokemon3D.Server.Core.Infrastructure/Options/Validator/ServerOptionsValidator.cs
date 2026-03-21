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

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Options.Validator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServerOptionsValidator(this IServiceCollection services)
    {
        services.TryAddSingleton<IValidateOptions<ServerOptions>, ServerOptionsValidator>();
        return services;
    }
}

internal class ServerOptionsValidator : IValidateOptions<ServerOptions>
{
    public ValidateOptionsResult Validate(string? name, ServerOptions options)
    {
        var failureMessages = new List<string>();

        if (!IPEndPoint.TryParse(options.BindingInformation, out var _))
        {
            // This is not ip address so let resolve this via hostname instead.
            try
            {
                if (Dns.GetHostAddresses(options.BindingInformation).Length == 0)
                {
                    failureMessages.Add($"[Server:{nameof(options.BindingInformation)}] Invalid hostname.");
                }
            }
            catch (Exception ex)
            {
                failureMessages.Add($"[Server:{nameof(options.BindingInformation)}] {ex.Message}");
            }
        }
        
        if (options.UpnpDiscoveryTime < 1)
        {
            failureMessages.Add($"[Server:{nameof(options.UpnpDiscoveryTime)}] Upnp Discovery Time require at least 1 second.");
        }

        if (string.IsNullOrEmpty(options.ServerName))
        {
            failureMessages.Add($"[Server:{nameof(options.ServerName)}] Server name is required.");
        }

        if (options.MaxPlayers is 0 or < -1)
        {
            failureMessages.Add($"[Server:{nameof(options.MaxPlayers)}] Max player limit must be more than 0.");
        }
        
        if (options.AwayFromKeyboardKickTime < 0)
        {
            failureMessages.Add($"[Server:{nameof(options.NoPingKickTime)}] Invalid AFK kick time.");
        }
        
        if (options.NoPingKickTime < 10)
        {
            failureMessages.Add($"[Server:{nameof(options.NoPingKickTime)}] No ping kick time must be at least 10 seconds.");
        }
        
        return failureMessages.Count > 0 ? ValidateOptionsResult.Fail(failureMessages) : ValidateOptionsResult.Success;
    }
}