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

using System.Collections.ObjectModel;
using Mediator;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TheDialgaTeam.Pokemon3D.Server.Core.Player;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Events;
using TheDialgaTeam.Pokemon3D.Server.Core.Player.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Cli.ViewModels;

public sealed class PlayerListViewModel :
    INotificationHandler<PlayerJoin>,
    INotificationHandler<PlayerUpdated>,
    INotificationHandler<PlayerLeft>
{
    public class Player : ReactiveObject
    {
        public int Id => _player.Id;
        
        [Reactive]
        private string GetDisplayStatus { get; set; }

        private readonly IPlayer _player;

        public Player(IPlayer player)
        {
            _player = player;
            GetDisplayStatus = $"{_player.Id}: {(_player.BusyType == BusyType.NotBusy ? _player.DisplayName : $"{_player.DisplayName} - {Enum.GetName(_player.BusyType)}")}";
        }

        public void Update()
        {
            GetDisplayStatus = $"{_player.Id}: {(_player.BusyType == BusyType.NotBusy ? _player.DisplayName : $"{_player.DisplayName} - {Enum.GetName(_player.BusyType)}")}";
        }

        public override string ToString()
        {
            return GetDisplayStatus;
        }
    }

    public ObservableCollection<Player> Players { get; } = [];

    private readonly object _syncLock = new();

    public ValueTask Handle(PlayerJoin notification, CancellationToken cancellationToken)
    {
        lock (_syncLock)
        {
            Players.Add(new Player(notification.Player));
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(PlayerUpdated notification, CancellationToken cancellationToken)
    {
        lock (_syncLock)
        {
            GetPlayerById(notification.Player.Id).Update();
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(PlayerLeft notification, CancellationToken cancellationToken)
    {
        lock (_syncLock)
        {
            Players.Remove(GetPlayerById(notification.Player.Id));
        }

        return ValueTask.CompletedTask;
    }

    private Player GetPlayerById(int id)
    {
        return Players.Single(wrapper => wrapper.Id == id);
    }
}