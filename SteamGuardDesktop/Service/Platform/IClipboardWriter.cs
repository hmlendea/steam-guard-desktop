namespace SteamGuardDesktop.Service.Platform
{
    /// <summary>
    /// Provides access to the clipboard of the host platform.
    /// </summary>
    public interface IClipboardWriter
    {
        /// <summary>
        /// Places a value on the clipboard.
        /// </summary>
        /// <param name="value">The value to copy.</param>
        /// <returns><c>true</c> when the value was copied.</returns>
        bool Copy(string value);
    }
}
