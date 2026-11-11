using System;
using System.Runtime.InteropServices;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Resolves and reports the platform theme of the current host.
    /// </summary>
    public sealed class ThemeService(IPlatformThemeResolver themeResolver) : IThemeService
    {
        private static string CurrentDesktopVariable => "XDG_CURRENT_DESKTOP";

        private static string DesktopSessionVariable => "XDG_SESSION_DESKTOP";

        /// <inheritdoc />
        public PlatformTheme GetCurrentTheme() => themeResolver.Resolve(DescribeHost());

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
