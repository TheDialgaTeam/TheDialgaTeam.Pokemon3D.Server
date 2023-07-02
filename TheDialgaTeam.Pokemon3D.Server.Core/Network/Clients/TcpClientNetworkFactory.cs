﻿// Pokemon 3D Server Client
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

using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Clients;

internal sealed class TcpClientNetworkFactory
{
    private readonly IServiceProvider _provider;

    public TcpClientNetworkFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public TcpClientNetwork CreateTcpClientNetwork(TcpClient client)
    {
        return new TcpClientNetwork(_provider.GetRequiredService<ILogger<TcpClientNetwork>>(), _provider.GetRequiredService<IMediator>(), client);
    }
}