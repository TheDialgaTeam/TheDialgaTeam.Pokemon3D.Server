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
using Microsoft.Extensions.Configuration;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Options;

public sealed class NetworkOptions : BaseOptions
{
    public string BindingInformation { get; set; } = new IPEndPoint(IPAddress.Any, 15124).ToString();

    public bool UseUpnp { get; set; }
    
    public int UpnpDiscoveryTime { get; set; } = 5;

    public NetworkOptions(IConfiguration configuration) : base(configuration)
    {
    }

    protected override void OnConfigurationChange(IConfiguration configuration)
    {
        var networkConfiguration = configuration.GetSection("Server:Network");
        //BindingInformation = networkConfiguration.GetValue<string>(nameof(BindingInformation));
    }
}