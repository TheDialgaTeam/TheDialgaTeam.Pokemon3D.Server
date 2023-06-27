namespace TheDialgaTeam.Pokemon3D.Server.Core.Network.Options.Models;

public sealed class ServerOptions
{
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

public sealed class WorldOptions
{
    public int Season { get; set; } = -1;

    public int Weather { get; set; } = -1;

    public bool DoDayCycle { get; set; } = true;

    public int[] SeasonMonth { get; set; } = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

    public int[] WeatherSeason { get; set; } = { -1, -1, -1, -1 };
}

public sealed class ChatOptions
{
    public bool AllowChat { get; set; } = true;

    public string[] ChatChannels { get; set; } = { "All" };
}

public sealed class PvPOptions
{
    public bool AllowPvP { get; set; } = true;

    public bool AllowPvPValidation { get; set; } = true;
}

public sealed class TradeOptions
{
    public bool AllowTrade { get; set; } = true;

    public bool AllowTradeValidation { get; set; } = true;
}