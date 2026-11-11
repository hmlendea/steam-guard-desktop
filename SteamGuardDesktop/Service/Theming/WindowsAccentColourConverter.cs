using System;
using System.Globalization;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Converts the numeric accent colour reported by Windows into a hexadecimal value.
    /// </summary>
    public static class WindowsAccentColourConverter
    {
        private static string ColourFormat => "#{0:X2}{1:X2}{2:X2}";

        /// <summary>
        /// Converts a Windows colour reference, which stores the channels in BBGGRR order, into a hexadecimal value.
        /// </summary>
        /// <param name="colourReference">The colour reference reported by Windows.</param>
        /// <returns>The accent colour as a hexadecimal value.</returns>
        public static string ToHexString(uint colourReference)
        {
            int red = (int)(colourReference & 0xFF);
            int green = (int)((colourReference >> 8) & 0xFF);
            int blue = (int)((colourReference >> 16) & 0xFF);

            return string.Format(CultureInfo.InvariantCulture, ColourFormat, red, green, blue);
        }
    }
}
