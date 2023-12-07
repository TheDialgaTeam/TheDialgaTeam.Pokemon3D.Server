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

using Mediator;
using Microsoft.Extensions.Hosting;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Commands;

namespace TheDialgaTeam.Pokemon3D.Server.Cli.Services;

internal sealed class ServerHostedService : IHostedService
{
    private readonly IMediator _mediator;

    public ServerHostedService(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _mediator.Send(new StartServer(), cancellationToken).ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _mediator.Send(new StopServer(), cancellationToken).ConfigureAwait(false);
    }
}