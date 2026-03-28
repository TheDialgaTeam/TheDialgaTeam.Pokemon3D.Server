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

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Client;
using TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Listener.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Listener;

internal class TcpNetworkListener(IPEndPoint ipEndPoint) : INetworkListener
{
    public IPEndPoint LocalEndPoint => ipEndPoint;
    
    public IObservable<INetworkClient> ObserveConnections => _incomingClientSubject.AsObservable();
    public IObservable<bool> IsListening => _isListeningSubject.AsObservable();
    
    private readonly TcpListener _tcpListener = new(ipEndPoint);
    
    private bool _isListening;
    
    private readonly Subject<INetworkClient> _incomingClientSubject = new();
    private readonly BehaviorSubject<bool> _isListeningSubject = new(false);
    
    private IDisposable? _incomingClientDisposable;
    
    public void StartListening()
    {
        if (Interlocked.CompareExchange(ref _isListening, true, false) is not false) return;
        
        _tcpListener.Start();
        _isListeningSubject.OnNext(true);

        _incomingClientDisposable = Observable.FromAsync(async token =>
        {
            while (_tcpListener.Server.IsBound && !token.IsCancellationRequested)
            {
                try
                {
                    _incomingClientSubject.OnNext(new TcpNetworkClient(await _tcpListener.AcceptTcpClientAsync(token).ConfigureAwait(false)));
                }
                catch (OperationCanceledException)
                {
                }
            }
        }).Subscribe();
    }
    
    public void StopListening()
    {
        if (Interlocked.CompareExchange(ref _isListening, false, true) is not true) return;

        Debug.Assert(_incomingClientDisposable != null, nameof(_incomingClientDisposable) + " != null");
        _incomingClientDisposable.Dispose();
        
        _tcpListener.Stop();
        _isListeningSubject.OnNext(false);
    }
}