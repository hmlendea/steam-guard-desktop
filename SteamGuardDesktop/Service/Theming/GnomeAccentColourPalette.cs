using System;
using System.Collections.Generic;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Maps the accent colour names supported by GNOME to hexadecimal values.
    /// </summary>
    public static class GnomeAccentColourPalette
    {
        private static readonly Dictionary<string, string> Colours = new(StringComparer.OrdinalIgnoreCase)
        {
            { "blue", "#3584E4" },
            { "teal", "#2190A4" },
            { "green", "#3A944A" },
            { "yellow", "#C88800" },
            { "orange", "#ED5B00" },
            { "red", "#E62D42" },
            { "pink", "#D56199" },
            { "purple", "#9141AC" },
            { "slate", "#6F8396" }
        };

        /// <summary>
        /// Resolves the accent colour that matches a GNOME accent colour name.
        /// </summary>
        /// <param name="accentName">The accent colour name reported by GNOME, optionally quoted.</param>
        /// <returns>The accent colour as a hexadecimal value, or <c>null</c> when the name is unrecognised.</returns>
        public static string? FromAccentName(string accentName)
        {
            if (string.IsNullOrWhiteSpace(accentName))
            {
                return null;
            }

            string normalisedName = accentName.Trim().Trim('\'', '"');

            if (!Colours.TryGetValue(normalisedName, out string? accentColour))
            {
                return null;
            }

            return accentColour;
        }
    }
}
