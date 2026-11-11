using System;
using System.Globalization;
using System.Linq;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Reads the accent colour configured by a KDE Plasma session.
    /// </summary>
    public static class KdeAccentColourReader
    {
        private static string GeneralSectionHeader => "[General]";

        private static string AccentColourKey => "AccentColor";

        private static string NormalisedColourFormat => "#{0:X2}{1:X2}{2:X2}";

        /// <summary>
        /// Reads the accent colour declared in the content of a KDE configuration file.
        /// </summary>
        /// <param name="configurationContent">The content of the KDE configuration file.</param>
        /// <returns>The accent colour as a hexadecimal value, or <c>null</c> when no accent colour is declared.</returns>
        public static string? Read(string configurationContent)
        {
            if (string.IsNullOrWhiteSpace(configurationContent))
            {
                return null;
            }

            string? declaredValue = FindDeclaredValue(configurationContent);

            if (declaredValue is null)
            {
                return null;
            }

            return ConvertDeclaredValue(declaredValue);
        }

        private static string? FindDeclaredValue(string configurationContent)
        {
            bool isInsideGeneralSection = false;

            foreach (string line in configurationContent.Split('\n'))
            {
                string trimmedLine = line.Trim();

                if (trimmedLine.StartsWith('['))
                {
                    isInsideGeneralSection = string.Equals(
                        trimmedLine,
                        GeneralSectionHeader,
                        StringComparison.OrdinalIgnoreCase);

                    continue;
                }

                if (!isInsideGeneralSection)
                {
                    continue;
                }

                int separatorIndex = trimmedLine.IndexOf('=');

                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = trimmedLine[..separatorIndex].Trim();

                if (!string.Equals(key, AccentColourKey, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return trimmedLine[(separatorIndex + 1)..].Trim();
            }

            return null;
        }

        private static string? ConvertDeclaredValue(string declaredValue)
        {
            int[] channels = [.. declaredValue
                .Split(',')
                .Select(channel => ParseChannel(channel))
                .Where(channel => channel >= 0)];

            if (channels.Length != 3)
            {
                return null;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                NormalisedColourFormat,
                channels[0],
                channels[1],
                channels[2]);
        }

        private static int ParseChannel(string channel)
        {
            if (!int.TryParse(channel.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedChannel))
            {
                return -1;
            }

            if (parsedChannel < 0 || parsedChannel > 255)
            {
                return -1;
            }

            return parsedChannel;
        }
    }
}
