using System;
using System.Collections.Generic;
using System.Linq;

using SteamGuardDesktop.DataAccess.Accounts;

using AccountModel = SteamGuardDesktop.Domain.Account.SteamAccount;

namespace SteamGuardDesktop.UnitTests.Utils
{
    /// <summary>
    /// Provides an account store held in memory.
    /// </summary>
    internal sealed class InMemoryAccountStore : IAccountStore
    {
        private readonly List<AccountModel> accounts = [];

        /// <inheritdoc />
        public IEnumerable<AccountModel> GetAll() => accounts.ToArray();

        /// <inheritdoc />
        public AccountModel Save(AccountModel account)
        {
            ArgumentNullException.ThrowIfNull(account, nameof(account));

            int existingIndex = accounts.FindIndex(candidate =>
                string.Equals(candidate.Identifier, account.Identifier, StringComparison.Ordinal));

            if (existingIndex < 0)
            {
                accounts.Add(account);
            }
            else
            {
                accounts[existingIndex] = account;
            }

            return account;
        }

        /// <inheritdoc />
        public bool Remove(string accountIdentifier) =>
            accounts.RemoveAll(candidate =>
                string.Equals(candidate.Identifier, accountIdentifier, StringComparison.Ordinal)) > 0;

        /// <inheritdoc />
        public bool Contains(string accountIdentifier) =>
            accounts.Any(candidate =>
                string.Equals(candidate.Identifier, accountIdentifier, StringComparison.Ordinal));

        /// <inheritdoc />
        public void Clear() => accounts.Clear();
    }
}