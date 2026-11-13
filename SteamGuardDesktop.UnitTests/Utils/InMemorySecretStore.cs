using System;
using System.Collections.Generic;

using SteamGuardDesktop.DataAccess.Accounts;
using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Service.Storage;

namespace SteamGuardDesktop.UnitTests.Utils
{
    /// <summary>
    /// Provides an in-memory secret store whose availability is supplied by the test.
    /// </summary>
    internal sealed class InMemorySecretStore : ISecretStore
    {
        private readonly Dictionary<string, string> entries = [];

        private readonly SecretStoreAvailability availability;

        /// <summary>
        /// Initialises a new instance of the <see cref="InMemorySecretStore"/> class.
        /// </summary>
        /// <param name="availability">The availability reported by the store.</param>
        public InMemorySecretStore(SecretStoreAvailability availability = SecretStoreAvailability.Available)
            => this.availability = availability;

        /// <inheritdoc />
        public SecretStoreAvailability GetAvailability() => availability;

        /// <inheritdoc />
        public SecretStoreResult Store(string entryName, string value)
        {
            if (availability != SecretStoreAvailability.Available)
            {
                return SecretStoreResult.Failed(availability, "The credential store is unavailable.");
            }

            entries[entryName] = value;

            return SecretStoreResult.Successful();
        }

        /// <inheritdoc />
        public string? Retrieve(string entryName)
        {
            if (!entries.TryGetValue(entryName, out string? value))
            {
                return null;
            }

            return value;
        }

        /// <inheritdoc />
        public SecretStoreResult Remove(string entryName)
        {
            entries.Remove(entryName);

            return SecretStoreResult.Successful();
        }
    }
}
