using System.ComponentModel;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Options;

public sealed class ServerOptions
{
    public string ProtocolVersion { get; set; } = "0.5";

    public string ServerName { get; set; } = "Pokemon 3D Server";

    public string ServerDescription { get; set; } = string.Empty;

    public string WelcomeMessage { get; set; } = string.Empty;

    public string[] GameModes { get; set; } = { "Kolben" };

    public int MaxPlayers { get; set; } = 20;

    public bool OfflineMode { get; set; } = false;

    public TimeSpan NoPingKickTime { get; set; } = TimeSpan.FromSeconds(10);
    
    public TimeSpan AwayFromKeyboardKickTime { get; set; } = TimeSpan.FromMinutes(5);

    public WorldOptions WorldOptions { get; set; } = new();

    public ChatOptions ChatOptions { get; set; } = new();

    public PvPOptions PvPOptions { get; set; } = new();

    public TradeOptions TradeOptions { get; set; } = new();
}