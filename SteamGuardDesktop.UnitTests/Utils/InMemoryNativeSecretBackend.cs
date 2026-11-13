using System.Collections.Generic;

using SteamGuardDesktop.Service.Storage;

namespace SteamGuardDesktop.UnitTests.Utils
{
    /// <summary>
    /// Provides an in-memory native credential store whose availability is supplied by the test.
    /// </summary>
    internal sealed class InMemoryNativeSecretBackend : INativeSecretBackend
    {
        private readonly Dictionary<string, string> entries = [];

        /// <summary>
        /// Initialises a new instance of the <see cref="InMemoryNativeSecretBackend"/> class.
        /// </summary>
        /// <param name="availability">The availability reported by the backend.</param>
        public InMemoryNativeSecretBackend(SecretStoreAvailability availability = SecretStoreAvailability.Available)
            => Availability = availability;

        /// <summary>
        /// Gets or sets the availability reported by the backend.
        /// </summary>
        public SecretStoreAvailability Availability { get; set; }

        /// <inheritdoc />
        public SecretStoreAvailability GetAvailability() => Availability;

        /// <inheritdoc />
        public string? Read(string entryName)
        {
            if (!entries.TryGetValue(entryName, out string? value))
            {
                return null;
            }

            return value;
        }

        /// <inheritdoc />
        public bool Write(string entryName, string value)
        {
            if (Availability != SecretStoreAvailability.Available)
            {
                return false;
            }

            entries[entryName] = value;

            return true;
        }

        /// <inheritdoc />
        public bool Delete(string entryName)
        {
            entries.Remove(entryName);

            return true;
        }
    }
}
