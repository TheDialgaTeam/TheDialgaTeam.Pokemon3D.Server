using TheDialgaTeam.Pokemon3D.Server.Core.Network.Clients;
using TheDialgaTeam.Pokemon3D.Server.Core.Network.Packages;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Client.Events;

public sealed class NewPackageReceivedEventArgs : EventArgs
{
    public required TcpClientNetwork Network { get; init; }
    
    public required Package Package { get; init; }
}