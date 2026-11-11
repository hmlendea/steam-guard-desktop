namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Persists and retrieves named secrets using the protected store of the host platform.
    /// </summary>
    public interface ISecretStore
    {
        /// <summary>
        /// Gets the current availability of the credential store.
        /// </summary>
        /// <returns>The availability of the credential store.</returns>
        SecretStoreAvailability GetAvailability();

        /// <summary>
        /// Stores a named secret.
        /// </summary>
        /// <param name="entryName">The credential store entry name.</param>
        /// <param name="value">The secret value to store.</param>
        /// <returns>The outcome of the operation.</returns>
        SecretStoreResult Store(string entryName, string value);

        /// <summary>
        /// Retrieves a named secret.
        /// </summary>
        /// <param name="entryName">The credential store entry name.</param>
        /// <returns>The stored value, or <c>null</c> when no value is stored.</returns>
        string? Retrieve(string entryName);

        /// <summary>
        /// Removes a named secret.
        /// </summary>
        /// <param name="entryName">The credential store entry name.</param>
        /// <returns>The outcome of the operation.</returns>
        SecretStoreResult Remove(string entryName);
    }
}
