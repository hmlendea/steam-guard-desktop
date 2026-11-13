using System;

using Avalonia;

namespace SteamGuardDesktop
{
    /// <summary>
    /// Provides the application entry point.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Starts the desktop application.
        /// </summary>
        /// <param name="arguments">The command-line arguments supplied to the process.</param>
        public static void Main(string[] arguments)
            => BuildAvaloniaApp().StartWithClassicDesktopLifetime(arguments);

        /// <summary>
        /// Builds the Avalonia application used by the desktop lifetime.
        /// </summary>
        /// <returns>The configured application builder.</returns>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder
                .Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
