using System.Security.Cryptography;

using SteamGuardDesktop.DataAccess.Accounts;

namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Creates the protected state stores, recovering from a file that can no longer be decrypted.
    /// </summary>
    /// <remarks>
    /// A protected file that was written with a different key, or that was truncated or tampered with, cannot be
    /// read. Such a file is moved aside so that the application starts with empty state instead of failing.
    /// </remarks>
    public static class ProtectedStateStoreFactory
    {
        /// <summary>
        /// Creates the account store, recovering from an unreadable file.
        /// </summary>
        /// <param name="filePath">The path of the encrypted account file.</param>
        /// <param name="keySeed">The machine-protected seed used to derive the encryption key.</param>
        /// <param name="wasRecovered">Whether an unreadable file had to be moved aside.</param>
        /// <returns>The account store.</returns>
        public static IAccountStore CreateAccountStore(string filePath, string keySeed, out bool wasRecovered)
        {
            try
            {
                IAccountStore accountStore = new EncryptedFileAccountStore(filePath, keySeed);
                wasRecovered = false;

                return accountStore;
            }
            catch (CryptographicException)
            {
                UnreadableStateFileRecovery.Quarantine(filePath);
                wasRecovered = true;

                return new EncryptedFileAccountStore(filePath, keySeed);
            }
        }

        /// <summary>
        /// Creates the secret store, recovering from an unreadable file.
        /// </summary>
        /// <param name="nativeBackend">The native credential store backend for the current host.</param>
        /// <param name="filePath">The path of the encrypted secret file.</param>
        /// <param name="keySeed">The machine-protected seed used to derive the encryption key.</param>
        /// <param name="wasRecovered">Whether an unreadable file had to be moved aside.</param>
        /// <returns>The secret store.</returns>
        public static ISecretStore CreateSecretStore(
            INativeSecretBackend nativeBackend,
            string filePath,
            string keySeed,
            out bool wasRecovered)
        {
            try
            {
                ISecretStore secretStore = new ResilientSecretStore(
                    nativeBackend,
                    new EncryptedFileSecretStore(filePath, keySeed));
                wasRecovered = false;

                return secretStore;
            }
            catch (CryptographicException)
            {
                UnreadableStateFileRecovery.Quarantine(filePath);
                wasRecovered = true;

                return new ResilientSecretStore(
                    nativeBackend,
                    new EncryptedFileSecretStore(filePath, keySeed));
            }
        }
    }
}
