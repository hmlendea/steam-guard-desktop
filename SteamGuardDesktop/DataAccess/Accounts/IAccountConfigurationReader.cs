using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Domain.Authenticator;

namespace SteamGuardDesktop.DataAccess.Accounts
{
    /// <summary>
    /// Reads an account configuration file so that it can be imported.
    /// </summary>
    public interface IAccountConfigurationReader
    {
        /// <summary>
        /// Reads an account configuration file.
        /// </summary>
        /// <param name="filePath">The path of the configuration file.</param>
        /// <param name="account">The account metadata described by the file, when reading succeeds.</param>
        /// <param name="secrets">The secrets described by the file, when reading succeeds.</param>
        /// <returns>The outcome of the operation.</returns>
        AccountConfigurationReadResult Read(
            string filePath,
            out SteamAccount? account,
            out AuthenticatorSecrets? secrets);

        /// <summary>
        /// Removes a configuration file after it has been imported.
        /// </summary>
        /// <param name="filePath">The path of the configuration file.</param>
        /// <returns><c>true</c> when the file no longer exists.</returns>
        bool Remove(string filePath);
    }
}
