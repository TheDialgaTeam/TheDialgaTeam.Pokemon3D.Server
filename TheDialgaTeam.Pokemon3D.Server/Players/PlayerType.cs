namespace TheDialgaTeam.Pokemon3D.Server.Players
{
    internal enum PlayerType
    {
        /// <summary>
        /// Normal Player
        /// </summary>
        Player,

        /// <summary>
        /// GameJolt Player
        /// </summary>
        GameJoltPlayer,

        /// <summary>
        /// Player with Chat Moderator ability
        /// </summary>
        ChatModerator,

        /// <summary>
        /// Player with Server Moderator ability
        /// </summary>
        ServerModerator,

        /// <summary>
        /// Player with Administrator ability
        /// </summary>
        Administrator,

        /// <summary>
        /// Player with Administrator ability and Debugging ability
        /// </summary>
        Creator
    }
}