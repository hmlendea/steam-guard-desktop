namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Describes the host platform and desktop session used to resolve a platform theme.
    /// </summary>
    public sealed class PlatformDescription
    {
        /// <summary>
        /// Gets or sets a value indicating whether the host is Windows.
        /// </summary>
        public bool IsWindows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the host is macOS.
        /// </summary>
        public bool IsMacOs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the host is Linux.
        /// </summary>
        public bool IsLinux { get; set; }

        /// <summary>
        /// Gets or sets the desktop session name reported by the host, such as <c>gnome</c> or <c>KDE</c>.
        /// </summary>
        public string DesktopEnvironment { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current desktop environment reported by the host, such as <c>GNOME</c> or <c>KDE</c>.
        /// </summary>
        public string CurrentDesktop { get; set; } = string.Empty;
    }
}
