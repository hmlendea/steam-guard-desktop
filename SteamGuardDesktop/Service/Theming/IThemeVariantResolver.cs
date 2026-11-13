using SteamGuardDesktop.Configuration;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Converts between a theme preference and the theme variant that the interface applies.
    /// </summary>
    public interface IThemeVariantResolver
    {
        /// <summary>
        /// Resolves the theme variant that matches a theme preference.
        /// </summary>
        /// <param name="themePreference">The theme preference selected by the user.</param>
        /// <returns>The name of the theme variant to apply.</returns>
        string ResolveVariantName(ThemePreference themePreference);

        /// <summary>
        /// Resolves the theme preference that a variant of the host reporting represents.
        /// </summary>
        /// <param name="hostPrefersDark">A value indicating whether the host reports a dark preference.</param>
        /// <returns>The theme preference that matches the host preference.</returns>
        ThemePreference ResolveFollowedPreference(bool hostPrefersDark);
    }
}
