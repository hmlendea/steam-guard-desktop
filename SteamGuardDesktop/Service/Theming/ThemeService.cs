using Avalonia;
using Avalonia.Styling;

using SteamGuardDesktop.Configuration;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Applies the single shared dark theme on every operating system and desktop environment.
    /// </summary>
    public sealed class ThemeService : IThemeService
    {
        /// <inheritdoc />
        public AppTheme ApplyTheme()
        {
            if (Application.Current is not null)
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
            }

            return AppTheme.Standard;
        }

        /// <inheritdoc />
        public AppTheme GetCurrentTheme() => AppTheme.Standard;
    }
}
