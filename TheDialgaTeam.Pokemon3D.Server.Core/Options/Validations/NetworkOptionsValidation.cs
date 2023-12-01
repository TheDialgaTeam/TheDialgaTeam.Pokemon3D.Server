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

using System.Net;
using Microsoft.Extensions.Options;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options.Validations;

internal sealed class NetworkOptionsValidation : IValidateOptions<NetworkOptions>
{
    public ValidateOptionsResult Validate(string? name, NetworkOptions options)
    {
        var failureMessages = new List<string>();
        
        if (!IPEndPoint.TryParse(options.BindingInformation, out var ipEndPoint))
        {
            failureMessages.Add($"[Server:Network:{nameof(options.BindingInformation)}] Invalid format given.");
        }
        
        if (!Dns.GetHostAddresses(Dns.GetHostName())
                .Append(IPAddress.Any)
                .Append(IPAddress.Loopback)
                .Any(address => ipEndPoint.Address.Equals(address)))
        {
            failureMessages.Add($"[Server:Network:{nameof(options.BindingInformation)}] Invalid IP address given.");
        }

        if (options.UpnpDiscoveryTime.TotalSeconds < 1)
        {
            failureMessages.Add($"[Server:Network:{nameof(options.UpnpDiscoveryTime)}] Upnp Discovery Time require at least 1 second.");
        }

        return failureMessages.Count > 0 ? ValidateOptionsResult.Fail(failureMessages) : ValidateOptionsResult.Success;
    }
}