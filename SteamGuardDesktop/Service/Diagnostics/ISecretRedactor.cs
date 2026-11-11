namespace SteamGuardDesktop.Service.Diagnostics
{
    /// <summary>
    /// Replaces sensitive values with a mask so that diagnostics never contain account secrets.
    /// </summary>
    public interface ISecretRedactor
    {
        /// <summary>
        /// Masks a sensitive value so that it cannot be read from diagnostics.
        /// </summary>
        /// <param name="value">The sensitive value to mask.</param>
        /// <returns>A safe representation of the value.</returns>
        string Mask(string value);

        /// <summary>
        /// Removes the query string and fragment of an address so that request parameters cannot be read from diagnostics.
        /// </summary>
        /// <param name="address">The address to sanitise.</param>
        /// <returns>The sanitised address.</returns>
        string SanitiseAddress(string address);
    }
}
