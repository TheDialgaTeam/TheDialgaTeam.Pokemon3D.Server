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
using TheDialgaTeam.Pokemon3D.Server.Core.Options.Implementations.Models;
using TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options.Implementations.Validations;

internal sealed class NetworkOptionsValidation : IValidateOptions<NetworkOptions>
{
    public ValidateOptionsResult Validate(string? name, NetworkOptions options)
    {
        var validNetworkInterfaces = NetworkUtility.GetLocalIpAddress();

        if (!validNetworkInterfaces
                .Append(IPAddress.Any)
                .Append(IPAddress.Loopback)
                .Any(address => options.BindIpEndPoint.Address.Equals(address)))
        {
            return ValidateOptionsResult.Fail($"[Network:{nameof(options.BindIpEndPoint)}] Invalid IP Address given.");
        }
        
        return ValidateOptionsResult.Success;
    }
}