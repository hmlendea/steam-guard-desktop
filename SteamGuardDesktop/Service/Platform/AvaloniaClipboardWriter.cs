using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace SteamGuardDesktop.Service.Platform
{
    /// <summary>
    /// Places values on the clipboard of the host platform through Avalonia.
    /// </summary>
    public sealed class AvaloniaClipboardWriter : IClipboardWriter
    {
        /// <inheritdoc />
        public bool Copy(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime)
            {
                return false;
            }

            Window? window = lifetime.MainWindow;

            if (window?.Clipboard is null)
            {
                return false;
            }

            window.Clipboard.SetTextAsync(value).GetAwaiter().GetResult();

            return true;
        }
    }
}
