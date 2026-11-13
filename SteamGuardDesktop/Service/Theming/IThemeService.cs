using SteamGuardDesktop.Configuration;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Applies the single shared visual theme that the application uses on every host.
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Applies the shared theme to the application.
        /// </summary>
        /// <returns>The applied theme.</returns>
        AppTheme ApplyTheme();

        /// <summary>
        /// Gets the theme that the application uses.
        /// </summary>
        /// <returns>The applied theme.</returns>
        AppTheme GetCurrentTheme();
    }
}
