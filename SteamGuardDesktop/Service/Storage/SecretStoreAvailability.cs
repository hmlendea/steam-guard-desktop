namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Reports whether the platform credential store can be used.
    /// </summary>
    public enum SecretStoreAvailability
    {
        /// <summary>
        /// The credential store is available and can be used.
        /// </summary>
        Available = 0,

        /// <summary>
        /// The credential store exists but is locked until the user unlocks the host session.
        /// </summary>
        Locked = 1,

        /// <summary>
        /// No credential store is available on the host platform.
        /// </summary>
        Unavailable = 2
    }
}
