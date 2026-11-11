namespace SteamGuardDesktop.Configuration
{
    /// <summary>
    /// Specifies the theme preference expressed by the user.
    /// </summary>
    public enum ThemePreference
    {
        /// <summary>
        /// The theme of the host operating system is followed.
        /// </summary>
        System = 0,

        /// <summary>
        /// The light variant of the platform theme is always used.
        /// </summary>
        Light = 1,

        /// <summary>
        /// The dark variant of the platform theme is always used.
        /// </summary>
        Dark = 2
    }
}
