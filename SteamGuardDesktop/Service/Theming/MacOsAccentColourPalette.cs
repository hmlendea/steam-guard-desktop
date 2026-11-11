using System.Collections.Generic;
using System.Globalization;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Maps the numeric accent colour identifier reported by macOS to a hexadecimal value.
    /// </summary>
    public static class MacOsAccentColourPalette
    {
        private static readonly Dictionary<int, string> Colours = new()
        {
            { -1, "#8E8E93" },
            { 0, "#FF5257" },
            { 1, "#F7821B" },
            { 2, "#FFC600" },
            { 3, "#62BA46" },
            { 4, "#007AFF" },
            { 5, "#A550A7" },
            { 6, "#F74F9E" },
            { 7, "#8E8E93" }
        };

        /// <summary>
        /// Resolves the accent colour that matches a macOS accent colour identifier.
        /// </summary>
        /// <param name="reportedIdentifier">The identifier reported by macOS.</param>
        /// <returns>The accent colour as a hexadecimal value, or <c>null</c> when the identifier is unrecognised.</returns>
        public static string? FromReportedIdentifier(int reportedIdentifier)
        {
            if (!Colours.TryGetValue(reportedIdentifier, out string? accentColour))
            {
                return null;
            }

            return accentColour;
        }

        /// <summary>
        /// Parses the textual output of the macOS accent colour query.
        /// </summary>
        /// <param name="reportedValue">The value reported by the host.</param>
        /// <returns>The accent colour as a hexadecimal value, or <c>null</c> when the value cannot be read.</returns>
        public static string? Parse(string reportedValue)
        {
            if (string.IsNullOrWhiteSpace(reportedValue))
            {
                return null;
            }

            if (!int.TryParse(reportedValue.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int identifier))
            {
                return null;
            }

            return FromReportedIdentifier(identifier);
        }
    }
}
