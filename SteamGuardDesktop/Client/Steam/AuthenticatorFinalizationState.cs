namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Represents the state of an authenticator finalisation reported by Steam.
    /// </summary>
    public enum AuthenticatorFinalizationState
    {
        /// <summary>
        /// Steam did not report a usable state.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The authenticator was activated.
        /// </summary>
        Succeeded = 1,

        /// <summary>
        /// Steam accepted the attempt but expects it to be repeated with a newer activation code.
        /// </summary>
        WantsMore = 2
    }
}
