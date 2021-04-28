namespace TheDialgaTeam.Pokemon3D.Server.Players
{
    public enum BusyType
    {
        /// <summary>
        /// Not Busy
        /// </summary>
        NotBusy = 0,

        /// <summary>
        /// Battling
        /// </summary>
        Battling = 1,

        /// <summary>
        /// Chatting
        /// </summary>
        Chatting = 2,

        /// <summary>
        /// Inactive - AFK
        /// </summary>
        Inactive = 3
    }
}