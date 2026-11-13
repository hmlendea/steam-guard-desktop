namespace SteamGuardDesktop.Configuration
{
    /// <summary>
    /// Represents the user-configurable settings of the application.
    /// </summary>
    public sealed class SteamGuardSettings
    {
        /// <summary>
        /// Gets or sets the timeout applied to Steam web requests, in seconds.
        /// </summary>
        public int NetworkTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum accepted size of a Steam web response, in bytes.
        /// </summary>
        public int MaximumResponseBytes { get; set; } = 1048576;

        /// <summary>
        /// Gets or sets a value indicating whether account details are hidden while the window is inactive.
        /// </summary>
        public bool IsAccountHiddenWhenInactive { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether secrets may be revealed on explicit request.
        /// </summary>
        public bool IsSecretRevealAllowed { get; set; }

        /// <summary>
        /// Gets or sets the base address of the Steam web API.
        /// </summary>
        public string SteamApiBaseAddress { get; set; } = "https://api.steampowered.com/";

        /// <summary>
        /// Gets or sets the base address of the Steam community site.
        /// </summary>
        public string SteamCommunityBaseAddress { get; set; } = "https://steamcommunity.com/";
    }
}
