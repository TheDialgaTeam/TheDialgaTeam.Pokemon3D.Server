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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Options;
using TheDialgaTeam.Pokemon3D.Server.Core.Security.PasswordHashing.Providers;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Security.PasswordHashing;

internal sealed class PasswordHashingFactory : IPasswordHashingFactory
{
    private readonly SecurityOptions _options;
    private readonly IServiceProvider _provider;
    
    public PasswordHashingFactory(IOptions<SecurityOptions> options, IServiceProvider provider)
    {
        _options = options.Value;
        _provider = provider;
    }

    public IPasswordHashingProvider GetProvider()
    {
        return _options.PasswordHashingProvider switch
        {
            Pbkdf2Options.ProviderName => ActivatorUtilities.CreateInstance<Pbkdf2PasswordHashingProvider>(_provider, _options.Pbkdf2),
            Argon2Options.ProviderName => ActivatorUtilities.CreateInstance<Argon2PasswordHashingProvider>(_provider, _options.Argon2),
            var _ => throw new NotImplementedException()
        };
    }
}