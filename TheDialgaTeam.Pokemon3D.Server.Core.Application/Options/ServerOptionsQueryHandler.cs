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

using Mediator;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options.Provider;
using TheDialgaTeam.Pokemon3D.Server.Core.Application.Options.Query;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Application.Options;

public class ServerOptionsQueryHandler(IServerOptionsProvider provider) : 
    IQueryHandler<GetServerOptions, ServerOptions>,
    IQueryHandler<GetGameModeOptions, GameModeOverrideOptions>
{
    public ValueTask<ServerOptions> Handle(GetServerOptions query, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(provider.GetOptions());
    }

    public ValueTask<GameModeOverrideOptions> Handle(GetGameModeOptions query, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(provider.GetGameModeOverrideOptions(query.GameMode));
    }
}