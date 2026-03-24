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

public class TcpNetworkListener : INetworkListener
{
    public IObservable<INetworkClient> ObserveConnections(IPEndPoint ipEndPoint)
    {
        return Observable.Create<INetworkClient>(async (observer, cancellationToken) =>
        {
            var disposable = new CompositeDisposable();
            var tcpListener = new TcpListener(ipEndPoint);
            
            disposable.Add(tcpListener);
            
            try
            {
                tcpListener.Start();
                
                while (tcpListener.Server.Connected && !cancellationToken.IsCancellationRequested)
                {
                    observer.OnNext(new TcpNetworkClient(await tcpListener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false)));
                }

                observer.OnCompleted();
            }
            catch (Exception exception)
            {
                observer.OnError(exception);
            }

            return disposable;
        });
    }
}