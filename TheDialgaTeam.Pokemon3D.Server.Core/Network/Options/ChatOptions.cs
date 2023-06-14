namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Options;

public sealed class ChatOptions
{
    public bool AllowChat { get; set; } = true;

    public string[] ChatChannels { get; set; } = { "All" };
}