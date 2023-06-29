namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Clients.Events;

public sealed class DisconnectedEventArgs : EventArgs
{
    public required TcpClientNetwork Network { get; init; }
}