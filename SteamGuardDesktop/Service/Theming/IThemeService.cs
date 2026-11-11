using SteamGuardDesktop.Configuration;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Reports the host platform, resolves the platform theme, and applies the requested theme variant.
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Gets the platform theme that matches the current host.
        /// </summary>
        /// <returns>The resolved platform theme.</returns>
        PlatformTheme GetCurrentTheme();

        /// <summary>
        /// Applies the requested theme preference and returns the resulting platform theme.
        /// </summary>
        /// <param name="themePreference">The theme preference selected by the user.</param>
        /// <returns>The platform theme that is now applied.</returns>
        PlatformTheme ApplyTheme(ThemePreference themePreference);

        /// <summary>
        /// Gets the accent colour configured in the host operating system.
        /// </summary>
        /// <returns>The accent colour as a hexadecimal value, or <c>null</c> when the host does not expose one.</returns>
        string? GetAccentColour();

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
