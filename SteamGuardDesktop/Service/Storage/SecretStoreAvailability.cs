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
        /// No credential store is available on the host platform, or the credential store cannot be reached.
        /// </summary>
        Unavailable = 1
    }
}
