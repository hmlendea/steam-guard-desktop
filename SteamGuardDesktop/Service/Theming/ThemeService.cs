using System;
using System.Runtime.InteropServices;

using Avalonia;
using Avalonia.Styling;

using SteamGuardDesktop.Configuration;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Resolves, applies, and reports the platform theme of the current host.
    /// </summary>
    public sealed class ThemeService(
        IPlatformThemeResolver themeResolver,
        ISystemAccentColourProvider accentColourProvider) : IThemeService
    {
        private static string CurrentDesktopVariable => "XDG_CURRENT_DESKTOP";

        private static string DesktopSessionVariable => "XDG_SESSION_DESKTOP";

        /// <inheritdoc />
        public PlatformTheme GetCurrentTheme() => themeResolver.Resolve(DescribeHost());

        /// <inheritdoc />
        public PlatformTheme ApplyTheme(ThemePreference themePreference)
        {
            ThemeVariant variant = ResolveThemeVariant(themePreference);

            if (Application.Current is not null)
            {
                Application.Current.RequestedThemeVariant = variant;
            }

            return GetCurrentTheme();
        }

        /// <inheritdoc />
        public string? GetAccentColour() => accentColourProvider.GetAccentColour();

        /// <inheritdoc />
        public string GetDesktopEnvironment()
        {
            string currentDesktop = Environment.GetEnvironmentVariable(CurrentDesktopVariable) ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(currentDesktop))
            {
                return currentDesktop;
            }

            return Environment.GetEnvironmentVariable(DesktopSessionVariable) ?? string.Empty;
        }

        /// <inheritdoc />
        public bool IsUsingFallbackTheme() => GetCurrentTheme() == PlatformTheme.Fallback;

        private static ThemeVariant ResolveThemeVariant(ThemePreference themePreference)
        {
            if (themePreference == ThemePreference.Light)
            {
                return ThemeVariant.Light;
            }

            if (themePreference == ThemePreference.Dark)
            {
                return ThemeVariant.Dark;
            }

            return ThemeVariant.Default;
        }

        private static PlatformDescription DescribeHost() => new()
        {
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            IsMacOs = RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
            CurrentDesktop = Environment.GetEnvironmentVariable(CurrentDesktopVariable) ?? string.Empty,
            DesktopEnvironment = Environment.GetEnvironmentVariable(DesktopSessionVariable) ?? string.Empty
        };
    }
}
