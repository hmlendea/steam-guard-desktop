namespace SteamGuardDesktop.Service.Diagnostics
{
    /// <summary>
    /// Removes sensitive values from request addresses so that they cannot be read from diagnostics.
    /// </summary>
    public interface ISecretRedactor
    {
        /// <summary>
        /// Removes the query string and fragment of an address so that request parameters cannot be read from diagnostics.
        /// </summary>
        /// <param name="address">The address to sanitise.</param>
        /// <returns>The sanitised address.</returns>
        string SanitiseAddress(string address);
    }
}
