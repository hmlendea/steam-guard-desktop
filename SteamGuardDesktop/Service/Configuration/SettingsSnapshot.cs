namespace SteamGuardDesktop.Service.Configuration
{
    /// <summary>
    /// Represents the user settings that are persisted between runs.
    /// </summary>
    public sealed class SettingsSnapshot
    {
        /// <summary>
        /// Gets or sets a value indicating whether account details are hidden while the window is inactive.
        /// </summary>
        public bool IsAccountHiddenWhenInactive { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether secrets may be revealed on explicit request.
        /// </summary>
        public bool IsSecretRevealAllowed { get; set; }
    }
}
