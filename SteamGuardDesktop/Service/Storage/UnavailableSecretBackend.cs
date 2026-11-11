namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Represents a native credential store that cannot be used on the current host.
    /// </summary>
    public sealed class UnavailableSecretBackend : INativeSecretBackend
    {
        /// <inheritdoc />
        public SecretStoreAvailability GetAvailability() => SecretStoreAvailability.Unavailable;

        /// <inheritdoc />
        public string? Read(string entryName) => null;

        /// <inheritdoc />
        public bool Write(string entryName, string value) => false;

        /// <inheritdoc />
        public bool Delete(string entryName) => true;
    }
}
