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
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Extensions;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Client;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Listener.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Packets.Types;

namespace TheDialgaTeam.Pokemon3D.Server.Test.Network.Listener;

public class TcpNetworkListenerTest
{
    private static IHost CreateHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigurePokemonServerInfrastructure(builder => builder.UseTcpNetworkListener())
            .Build();
    }

    [Fact]
    public void SimpleStartStopTest()
    {
        var host = CreateHost();
        var factory = host.Services.GetRequiredService<INetworkListenerFactory>();
        var listener = factory.Create(new IPEndPoint(IPAddress.Loopback, 15124));

        IList<bool> isListening = [];
        listener.IsListening.Buffer(2).Subscribe(list => isListening = list);
        
        listener.StartListening();
        listener.StartListening();
        listener.StopListening();
        listener.StopListening();
        
        Assert.True(isListening.SequenceEqual([false, true]));
    }
}