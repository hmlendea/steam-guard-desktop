using SteamGuardDesktop.Configuration;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Converts between a theme preference and the theme variant that the interface applies.
    /// </summary>
    public sealed class ThemeVariantResolver : IThemeVariantResolver
    {
        private static string SystemVariantName => "Default";

        private static string LightVariantName => "Light";

        private static string DarkVariantName => "Dark";

        /// <inheritdoc />
        public string ResolveVariantName(ThemePreference themePreference)
        {
            if (themePreference == ThemePreference.Light)
            {
                return LightVariantName;
            }

            if (themePreference == ThemePreference.Dark)
            {
                return DarkVariantName;
            }

            return SystemVariantName;
        }

        /// <inheritdoc />
        public ThemePreference ResolveFollowedPreference(bool hostPrefersDark)
        {
            if (hostPrefersDark)
            {
                return ThemePreference.Dark;
            }

            return ThemePreference.Light;
        }
    }
}
