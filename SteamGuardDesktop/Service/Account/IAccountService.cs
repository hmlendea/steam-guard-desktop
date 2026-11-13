using System.Collections.Generic;

using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Service.Storage;

namespace SteamGuardDesktop.Service.Account
{
    /// <summary>
    /// Coordinates the configured accounts and their stored secrets.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Retrieves every configured account.
        /// </summary>
        /// <returns>The configured accounts.</returns>
        IEnumerable<SteamAccount> GetAccounts();

        /// <summary>
        /// Adds or updates an account and stores its secrets.
        /// </summary>
        /// <param name="account">The account metadata to save.</param>
        /// <param name="secrets">The secrets that belong to the account.</param>
        /// <returns>The outcome of the operation.</returns>
        AccountSaveResult SaveAccount(SteamAccount account, AuthenticatorSecrets secrets);

        /// <summary>
        /// Removes an account and every stored secret that belongs to it.
        /// </summary>
        /// <param name="accountIdentifier">The identifier of the account to remove.</param>
        /// <returns>The outcome of the operation.</returns>
        AccountRemovalResult RemoveAccount(string accountIdentifier);

        /// <summary>
        /// Retrieves the stored secrets of an account.
        /// </summary>
        /// <param name="account">The account whose secrets are requested.</param>
        /// <returns>The stored secrets, or <c>null</c> when they are unavailable.</returns>
        AuthenticatorSecrets? GetSecrets(SteamAccount account);

        /// <summary>
        /// Determines whether an account is already configured.
        /// </summary>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <returns><c>true</c> when the account is configured.</returns>
        bool IsAccountConfigured(string steamIdentifier);

        /// <summary>
        /// Stores the access token that authenticates confirmation requests for an account.
        /// </summary>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <param name="accessToken">The access token issued by Steam. This value is sensitive.</param>
        /// <returns>The outcome of the operation.</returns>
        AccountAccessTokenResult SaveAccessToken(string steamIdentifier, string accessToken);

        /// <summary>
        /// Reports the availability of the credential store.
        /// </summary>
        /// <returns>The availability of the credential store.</returns>
        SecretStoreAvailability GetSecretStoreAvailability();
    }
}
