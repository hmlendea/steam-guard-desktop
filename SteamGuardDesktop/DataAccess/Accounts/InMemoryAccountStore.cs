using System;
using System.Collections.Generic;
using System.Linq;

using SteamGuardDesktop.Domain.Account;

namespace SteamGuardDesktop.DataAccess.Accounts
{
    /// <summary>
    /// Stores non-secret account metadata in memory.
    /// </summary>
    public sealed class InMemoryAccountStore : IAccountStore
    {
        private readonly List<SteamAccount> accounts;

        /// <summary>
        /// Initialises a new instance of the <see cref="InMemoryAccountStore"/> class.
        /// </summary>
        public InMemoryAccountStore() => accounts = [];

        /// <inheritdoc />
        public IEnumerable<SteamAccount> GetAll() => accounts.ToArray();

        /// <inheritdoc />
        public SteamAccount Save(SteamAccount account)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(account.Identifier, nameof(account.Identifier));
            accounts.Add(account);

            return account;
        }

        /// <inheritdoc />
        public bool Remove(string accountIdentifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accountIdentifier, nameof(accountIdentifier));

            return accounts.RemoveAll(account => account.Identifier == accountIdentifier) > 0;
        }

        /// <inheritdoc />
        public bool Contains(string accountIdentifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accountIdentifier, nameof(accountIdentifier));

            return accounts.Any(account => account.Identifier == accountIdentifier);
        }

        /// <inheritdoc />
        public void Clear() => accounts.Clear();
    }
}
