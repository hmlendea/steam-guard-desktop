using System;
using System.Diagnostics;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Reads the accent colour of the current host operating system.
    /// </summary>
    public sealed class SystemAccentColourProvider : ISystemAccentColourProvider
    {
        private static string KdeConfigurationPath => ".config/kdeglobals";

        private static string GnomeAccentQuery => "get org.gnome.desktop.interface accent-color";

        private static string MacOsAccentQuery => "read -g AppleAccentColor";

        /// <inheritdoc />
        public string? GetAccentColour()
        {
            if (OperatingSystem.IsWindows())
            {
                return WindowsSystemAccentReader.Read();
            }

            if (OperatingSystem.IsMacOS())
            {
                return MacOsAccentColourPalette.Parse(RunCommand("defaults", MacOsAccentQuery) ?? string.Empty);
            }

            if (OperatingSystem.IsLinux())
            {
                return GetLinuxAccentColour();
            }

            return null;
        }

        private static string? GetLinuxAccentColour()
        {
            string? gnomeAccent = GnomeAccentColourPalette.FromAccentName(
                RunCommand("gsettings", GnomeAccentQuery) ?? string.Empty);

            if (gnomeAccent is not null)
            {
                return gnomeAccent;
            }

            string configurationPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                KdeConfigurationPath);

            if (!System.IO.File.Exists(configurationPath))
            {
                return null;
            }

            return KdeAccentColourReader.Read(System.IO.File.ReadAllText(configurationPath));
        }

        private static string? RunCommand(string fileName, string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new()
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? process = Process.Start(startInfo);

                if (process is null)
                {
                    return null;
                }

                string standardOutput = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    return null;
                }

                return standardOutput.Trim();
            }
            catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or InvalidOperationException)
            {
                return null;
            }
        }
    }
}
