namespace TheDialgaTeam.Pokemon3D.Server.Packages
{
    internal enum PackageType
    {
        /// <summary>
        /// Package Type: Unknown Data
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Package Type: Game Data
        /// <para>Join: {Origin = PlayerID | DataItem[] = FullPackageData[] | To other players}</para>
        /// <para>Update: {Origin = PlayerID | DataItem[] = PartialPackageData[] | To other players}</para>
        /// </summary>
        GameData = 0,

        /// <summary>
        /// Private Message
        /// <para>Global: {Origin = -1 | DataItem[0] = Message | To the player}</para>
        /// <para>Own: {Origin = PlayerID | DataItem[0] = PlayerID, DataItem[1] = Message | To yourself}</para>
        /// <para>Client: {Origin = PlayerID | DataItem[0] = Message | To client}</para>
        /// </summary>
        PrivateMessage = 2,

        /// <summary>
        /// Chat Message
        /// <para>Global: {Origin = -1 | DataItem[0] = Message | To all players}</para>
        /// <para>Player: {Origin = PlayerID | DataItem[0] = Message | To all players}</para>
        /// </summary>
        ChatMessage = 3,

        /// <summary>
        /// Kick
        /// <para>{Origin = -1 | DataItem[0] = Reason | To player}</para>
        /// </summary>
        Kicked = 4,

        /// <summary>
        /// ID
        /// <para>{Origin = -1 | DataItem[0] = PlayerID | To own}</para>
        /// </summary>
        Id = 7,

        /// <summary>
        /// Create Player
        /// <para>{Origin = -1 | DataItem[0] = PlayerID | To other players}</para>
        /// </summary>
        CreatePlayer = 8,

        /// <summary>
        /// Destroy Player
        /// <para>{Origin = -1 | DataItem[0] = PlayerID | To other players}</para>
        /// </summary>
        DestroyPlayer = 9,

        /// <summary>
        /// Server Close
        /// <para>{Origin = -1 | DataItem[0] = Reason | To all players}</para>
        /// </summary>
        ServerClose = 10,

        /// <summary>
        /// Server Message
        /// <para>{Origin = -1 | DataItem[0] = Message | To all players}</para>
        /// </summary>
        ServerMessage = 11,

        /// <summary>
        /// World Data
        /// <para>{Origin = -1 | DataItem[0] = Season, DataItem[1] = Weather, DataItem[2] = Time | To all players}</para>
        /// </summary>
        WorldData = 12,

        /// <summary>
        /// Ping (Get Only)
        /// </summary>
        Ping = 13,

        /// <summary>
        /// Gamestate Message (Get Only)
        /// </summary>
        GamestateMessage = 14,

        /// <summary>
        /// Trade Request
        /// <para>{Origin = PlayerID | DataItem = null | To trade player}</para>
        /// </summary>
        TradeRequest = 30,

        /// <summary>
        /// Trade Join
        /// <para>{Origin = PlayerID | DataItem = null | To trade player}</para>
        /// </summary>
        TradeJoin = 31,

        /// <summary>
        /// Trade Quit
        /// <para>{Origin = PlayerID | DataItem = null | To trade player}</para>
        /// </summary>
        TradeQuit = 32,

        /// <summary>
        /// Trade Offer
        /// <para>{Origin = PlayerID | DataItem[0] = PokemonData | To trade player}</para>
        /// </summary>
        TradeOffer = 33,

        /// <summary>
        /// Trade Start
        /// <para>{Origin = PlayerID | DataItem = null | To trade player}</para>
        /// </summary>
        TradeStart = 34,

        /// <summary>
        /// Battle Request
        /// <para>{Origin = PlayerID | DataItem = null | To battle player}</para>
        /// </summary>
        BattleRequest = 50,

        /// <summary>
        /// Battle Join
        /// <para>{Origin = PlayerID | DataItem = null | To battle player}</para>
        /// </summary>
        BattleJoin = 51,

        /// <summary>
        /// Battle Quit
        /// <para>{Origin = PlayerID | DataItem = null | To battle player}</para>
        /// </summary>
        BattleQuit = 52,

        /// <summary>
        /// Battle Offer
        /// <para>{Origin = PlayerID | DataItem[0] = PokemonData | To battle player}</para>
        /// </summary>
        BattleOffer = 53,

        /// <summary>
        /// Battle Start
        /// <para>{Origin = PlayerID | DataItem = null | To battle player}</para>
        /// </summary>
        BattleStart = 54,

        /// <summary>
        /// Battle Client Data
        /// <para>{Origin = PlayerID | DataItem[0] = ClientData | To battle player}</para>
        /// </summary>
        BattleClientData = 55,

        /// <summary>
        /// Battle Host Data
        /// <para>{Origin = PlayerID | DataItem[0] = HostData | To battle player}</para>
        /// </summary>
        BattleHostData = 56,

        /// <summary>
        /// Battle Pokemon Data
        /// <para>{Origin = PlayerID | DataItem[0] = PokemonData | To battle player}</para>
        /// </summary>
        BattlePokemonData = 57,

        /// <summary>
        /// Server Info Data
        /// <para>{Origin = -1 | DataItem[0] = Player Count, DataItem[1] = Max Player Count, DataItem[2] = Server Name, DataItem[3] = Server Description, DataItem[..] = Player Names | To listening client}</para>
        /// </summary>
        ServerInfoData = 98,

        /// <summary>
        /// Server Data Request (Read only)
        /// </summary>
        ServerDataRequest = 99
    }
}