using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Reads the accent colour configured in Windows.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class WindowsSystemAccentReader
    {
        private static uint AccentColourHintSetting => 0x1002;

        /// <summary>
        /// Reads the accent colour hint configured in the current Windows session.
        /// </summary>
        /// <returns>The accent colour as a hexadecimal value, or <c>null</c> when it cannot be read.</returns>
        public static string? Read()
        {
            AccentColourValue accentColourValue = new() { Size = Marshal.SizeOf<AccentColourValue>() };

            if (!SystemParametersInfo(AccentColourHintSetting, 0, ref accentColourValue, 0))
            {
                return null;
            }

            return WindowsAccentColourConverter.ToHexString(accentColourValue.Colour);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(
            uint action,
            uint parameter,
            ref AccentColourValue value,
            uint flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentColourValue
        {
            public int Size;

            public uint Colour;
        }
    }
}
