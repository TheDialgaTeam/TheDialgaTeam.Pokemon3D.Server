using TheDialgaTeam.Pokemon3D.Server.Core.Network.Clients;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Client.Events;

public sealed class DisconnectedEventArgs : EventArgs
{
    public required TcpClientNetwork Network { get; init; }
}