using System;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Resolves the platform theme that matches a host environment.
    /// </summary>
    public sealed class PlatformThemeResolver : IPlatformThemeResolver
    {
        private static string GnomeDesktopMarker => "gnome";

        private static string KdeDesktopMarker => "kde";

        /// <inheritdoc />
        public PlatformTheme Resolve(PlatformDescription description)
        {
            ArgumentNullException.ThrowIfNull(description, nameof(description));

            if (description.IsWindows)
            {
                return PlatformTheme.WindowsFluent;
            }

            if (description.IsMacOs)
            {
                return PlatformTheme.MacOsAqua;
            }

            if (!description.IsLinux)
            {
                return PlatformTheme.Fallback;
            }

            return ResolveLinuxTheme(description);
        }

        private static PlatformTheme ResolveLinuxTheme(PlatformDescription description)
        {
            string desktopName = ComposeDesktopName(description);

            if (desktopName.Length == 0)
            {
                return PlatformTheme.Fallback;
            }

            if (desktopName.Contains(GnomeDesktopMarker, StringComparison.Ordinal))
            {
                return PlatformTheme.GnomeAdwaita;
            }

            if (desktopName.Contains(KdeDesktopMarker, StringComparison.Ordinal))
            {
                return PlatformTheme.KdeBreeze;
            }

            return PlatformTheme.Fallback;
        }

        private static string ComposeDesktopName(PlatformDescription description) =>
            (description.CurrentDesktop + " " + description.DesktopEnvironment).Trim().ToLowerInvariant();
    }
}
