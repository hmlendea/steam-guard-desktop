namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Identifies the platform theme that matches the host environment.
    /// </summary>
    public enum PlatformTheme
    {
        /// <summary>
        /// The Windows Fluent theme.
        /// </summary>
        WindowsFluent = 0,

        /// <summary>
        /// The macOS Aqua theme.
        /// </summary>
        MacOsAqua = 1,

        /// <summary>
        /// The GNOME Adwaita theme.
        /// </summary>
        GnomeAdwaita = 2,

        /// <summary>
        /// The KDE Breeze theme.
        /// </summary>
        KdeBreeze = 3,

        /// <summary>
        /// The documented fallback theme used when the desktop environment cannot be identified.
        /// </summary>
        Fallback = 4
    }
}
