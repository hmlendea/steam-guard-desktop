namespace SteamGuardDesktop.ViewModels
{
    /// <summary>
    /// Describes how a status message should be presented to the user.
    /// </summary>
    public enum StatusMessageKind
    {
        /// <summary>
        /// No message is being presented.
        /// </summary>
        None = 0,

        /// <summary>
        /// A success that the user asked for and that completed.
        /// </summary>
        Success = 1,

        /// <summary>
        /// A failure that the user must act on.
        /// </summary>
        Failure = 2
    }
}
