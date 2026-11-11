namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Uses the platform credential store and falls back to an encrypted file when it is unavailable.
    /// </summary>
    public sealed class ResilientSecretStore(
        INativeSecretBackend nativeBackend,
        ISecretStore fallbackStore) : ISecretStore
    {
        /// <inheritdoc />
        public SecretStoreAvailability GetAvailability()
        {
            if (nativeBackend.GetAvailability() == SecretStoreAvailability.Available)
            {
                return SecretStoreAvailability.Available;
            }

            return fallbackStore.GetAvailability();
        }

        /// <inheritdoc />
        public SecretStoreResult Store(string entryName, string value)
        {
            if (nativeBackend.GetAvailability() == SecretStoreAvailability.Available)
            {
                SecretStoreResult nativeResult = new NativeSecretStore(nativeBackend).Store(entryName, value);

                if (nativeResult.IsSuccessful)
                {
                    fallbackStore.Remove(entryName);

                    return nativeResult;
                }
            }

            return fallbackStore.Store(entryName, value);
        }

        /// <inheritdoc />
        public string? Retrieve(string entryName)
        {
            if (nativeBackend.GetAvailability() == SecretStoreAvailability.Available)
            {
                string? nativeValue = new NativeSecretStore(nativeBackend).Retrieve(entryName);

                if (!string.IsNullOrWhiteSpace(nativeValue))
                {
                    return nativeValue;
                }
            }

            return fallbackStore.Retrieve(entryName);
        }

        /// <inheritdoc />
        public SecretStoreResult Remove(string entryName)
        {
            new NativeSecretStore(nativeBackend).Remove(entryName);

            return fallbackStore.Remove(entryName);
        }
    }
}
