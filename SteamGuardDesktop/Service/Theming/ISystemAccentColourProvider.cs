namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Supplies the accent colour configured in the host operating system.
    /// </summary>
    public interface ISystemAccentColourProvider
    {
        /// <summary>
        /// Retrieves the accent colour configured in the host operating system.
        /// </summary>
        /// <returns>The accent colour as a hexadecimal value, or <c>null</c> when the host does not expose one.</returns>
        string? GetAccentColour();
    }
}
