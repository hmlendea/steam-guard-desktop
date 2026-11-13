using System;

namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Stores named secrets in the native credential store of the host operating system.
    /// </summary>
    public sealed class NativeSecretStore(INativeSecretBackend backend) : ISecretStore
    {
        /// <inheritdoc />
        public SecretStoreAvailability GetAvailability() => backend.GetAvailability();

        /// <inheritdoc />
        public SecretStoreResult Store(string entryName, string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryName, nameof(entryName));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            SecretStoreAvailability availability = backend.GetAvailability();

            if (availability != SecretStoreAvailability.Available)
            {
                return SecretStoreResult.Failed(availability, "The platform credential store cannot be written.");
            }

            if (!backend.Write(entryName, value))
            {
                return SecretStoreResult.Failed(
                    SecretStoreAvailability.Unavailable,
                    "The platform credential store rejected the credential.");
            }

            return SecretStoreResult.Successful();
        }

        /// <inheritdoc />
        public string? Retrieve(string entryName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryName, nameof(entryName));

            return backend.Read(entryName);
        }

        /// <inheritdoc />
        public SecretStoreResult Remove(string entryName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryName, nameof(entryName));

            backend.Delete(entryName);

            return SecretStoreResult.Successful();
        }
    }
}
