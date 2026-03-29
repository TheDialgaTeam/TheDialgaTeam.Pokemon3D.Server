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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Client;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Listener;

internal static class TcpNetworkListener
{
    public static IObservable<INetworkClient> Create(IPEndPoint ipEndPoint, Action<TcpListener>? configure = null)
    {
        return Observable.Create<INetworkClient>(async (observer, token) =>
        {
            try
            {
                var listener = new TcpListener(ipEndPoint);
                configure?.Invoke(listener);
                listener.Start();
                
                while (listener.Server.IsBound && !token.IsCancellationRequested)
                {
                    try
                    {
                        observer.OnNext(new TcpNetworkClient(await listener.AcceptTcpClientAsync(token).ConfigureAwait(false)));
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
                
                observer.OnCompleted();
                listener.Stop();
            }
            catch (SocketException exception)
            {
                observer.OnError(exception);
            }
            
            return Disposable.Empty;
        });
    }
}