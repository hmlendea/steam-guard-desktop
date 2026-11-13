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
        ISystemAccentColourProvider accentColourProvider,
        IThemeVariantResolver themeVariantResolver) : IThemeService
    {
        private static string CurrentDesktopVariable => "XDG_CURRENT_DESKTOP";

        private static string DesktopSessionVariable => "XDG_SESSION_DESKTOP";

        /// <inheritdoc />
        public PlatformTheme GetCurrentTheme() => themeResolver.Resolve(DescribeHost());

        /// <inheritdoc />
        public PlatformTheme ApplyTheme(ThemePreference themePreference)
        {
            ApplyVariantName(themeVariantResolver.ResolveVariantName(themePreference));

            return GetCurrentTheme();
        }

        /// <inheritdoc />
        public void RefreshFromHost()
        {
            if (Application.Current is null)
            {
                return;
            }

            ApplyVariantName(Application.Current.ActualThemeVariant.Key as string ?? "Default");
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

        private static void ApplyVariantName(string variantName)
        {
            if (Application.Current is null)
            {
                return;
            }

            if (string.Equals(variantName, "Light", StringComparison.Ordinal))
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;

                return;
            }

            if (string.Equals(variantName, "Dark", StringComparison.Ordinal))
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Dark;

                return;
            }

            Application.Current.RequestedThemeVariant = ThemeVariant.Default;
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
