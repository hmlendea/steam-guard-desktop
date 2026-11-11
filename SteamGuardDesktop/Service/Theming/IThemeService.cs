namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Reports the host platform and the resolved platform theme.
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Gets the platform theme that matches the current host.
        /// </summary>
        /// <returns>The resolved platform theme.</returns>
        PlatformTheme GetCurrentTheme();

        /// <summary>
        /// Gets the desktop environment detected on the current host.
        /// </summary>
        /// <returns>The desktop environment name, or an empty string when it cannot be identified.</returns>
        string GetDesktopEnvironment();

        /// <summary>
        /// Gets a value indicating whether the resolved theme falls back to the documented default.
        /// </summary>
        /// <returns><c>true</c> when the fallback theme is in use.</returns>
        bool IsUsingFallbackTheme();
    }
}
