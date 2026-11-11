namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Provides access to the native credential store of the host operating system.
    /// </summary>
    public interface INativeSecretBackend
    {
        /// <summary>
        /// Reports whether the native credential store can be used.
        /// </summary>
        /// <returns>The availability of the native credential store.</returns>
        SecretStoreAvailability GetAvailability();

        /// <summary>
        /// Reads the value stored under the specified credential name.
        /// </summary>
        /// <param name="entryName">The name of the credential store entry.</param>
        /// <returns>The stored value, or <c>null</c> when the entry does not exist.</returns>
        string? Read(string entryName);

        /// <summary>
        /// Writes the value under the specified credential name.
        /// </summary>
        /// <param name="entryName">The name of the credential store entry.</param>
        /// <param name="value">The value to store.</param>
        /// <returns><c>true</c> when the value was stored.</returns>
        bool Write(string entryName, string value);

        /// <summary>
        /// Deletes the value stored under the specified credential name.
        /// </summary>
        /// <param name="entryName">The name of the credential store entry.</param>
        /// <returns><c>true</c> when the entry no longer exists.</returns>
        bool Delete(string entryName);
    }
}
