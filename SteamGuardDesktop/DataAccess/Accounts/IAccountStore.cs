using System.Collections.Generic;

using SteamGuardDesktop.Domain.Account;

namespace SteamGuardDesktop.DataAccess.Accounts
{
    /// <summary>
    /// Persists the non-secret account configuration between runs.
    /// </summary>
    public interface IAccountStore
    {
        /// <summary>
        /// Retrieves every configured account.
        /// </summary>
        /// <returns>The configured accounts.</returns>
        IEnumerable<SteamAccount> GetAll();

        /// <summary>
        /// Saves an account and returns it with a stable identifier.
        /// </summary>
        /// <param name="account">The account to save.</param>
        /// <returns>The saved account.</returns>
        SteamAccount Save(SteamAccount account);

        /// <summary>
        /// Removes a configured account and its local metadata.
        /// </summary>
        /// <param name="accountIdentifier">The identifier of the account to remove.</param>
        /// <returns><c>true</c> when the account existed.</returns>
        bool Remove(string accountIdentifier);

        /// <summary>
        /// Determines whether an account identifier is already in use.
        /// </summary>
        /// <param name="accountIdentifier">The identifier to test.</param>
        /// <returns><c>true</c> when the identifier is in use.</returns>
        bool Contains(string accountIdentifier);

        /// <summary>
        /// Discards every configured account.
        /// </summary>
        void Clear();
    }
}
