using System;
using System.Collections.Generic;
using System.Linq;

using NuciLog.Core;

using SteamGuardDesktop.DataAccess.Accounts;
using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Logging;
using SteamGuardDesktop.Service.Authenticator;
using SteamGuardDesktop.Service.Storage;

namespace SteamGuardDesktop.Service.Account
{
    /// <summary>
    /// Coordinates the configured accounts and their stored secrets.
    /// </summary>
    public sealed class AccountService(
        IAccountStore accountStore,
        ISecretStore secretStore,
        IDeviceIdentityProvider deviceIdentityProvider,
        IOtpCodeGenerator otpCodeGenerator,        IAccountConfigurationReader configurationReader,        ILogger logger) : IAccountService
    {
        private static string SecretEntryNamePrefix => "SteamGuardDesktop:";

        private static int SteamIdentifierLength => 17;

        /// <inheritdoc />
        public IEnumerable<SteamAccount> GetAccounts() => accountStore.GetAll();

        /// <inheritdoc />
        public AccountSaveResult SaveAccount(SteamAccount account, AuthenticatorSecrets secrets)
        {
            ArgumentNullException.ThrowIfNull(account, nameof(account));
            ArgumentNullException.ThrowIfNull(secrets, nameof(secrets));

            List<string> validationErrors = ValidateAccount(account, secrets);

            if (validationErrors.Count > 0)
            {
                logger.Warn(
                    SteamGuardOperation.StoreSecrets,
                    OperationStatus.Failure,
                    new LogInfo(SteamGuardInfoKey.FailureReason, string.Join(" ", validationErrors)));

                return AccountSaveResult.Invalid(validationErrors);
            }

            if (string.IsNullOrWhiteSpace(account.Identifier))
            {
                account.Identifier = Guid.NewGuid().ToString("N");
            }

            account.SteamIdentifier = account.SteamIdentifier.Trim();
            account.Label = string.IsNullOrWhiteSpace(account.Label) ? account.SteamIdentifier : account.Label.Trim();
            account.DeviceIdentifier = deviceIdentityProvider.DeriveDeviceIdentifier(account.SteamIdentifier);
            account.SharedSecretName = ComposeEntryName(account.Identifier, "shared");
            account.IdentitySecretName = ComposeEntryName(account.Identifier, "identity");
            account.RevocationCodeName = ComposeEntryName(account.Identifier, "revocation");
            account.AccessTokenName = ComposeEntryName(account.Identifier, "access-token");

            SecretStoreResult sharedSecretResult = secretStore.Store(account.SharedSecretName, secrets.SharedSecret.Trim());
            SecretStoreResult identitySecretResult = secretStore.Store(account.IdentitySecretName, secrets.IdentitySecret.Trim());

            if (!sharedSecretResult.IsSuccessful || !identitySecretResult.IsSuccessful)
            {
                secretStore.Remove(account.SharedSecretName);
                secretStore.Remove(account.IdentitySecretName);

                logger.Error(
                    SteamGuardOperation.StoreSecrets,
                    OperationStatus.Failure,
                    new LogInfo(SteamGuardInfoKey.SecretStoreAvailability, sharedSecretResult.Availability.ToString()));

                return AccountSaveResult.Failed("The secrets could not be stored in the platform credential store.");
            }

            StoreOptionalSecret(account.RevocationCodeName, secrets.RevocationCode);
            StoreOptionalSecret(account.AccessTokenName, secrets.AccessToken);

            accountStore.Save(account);

            logger.Info(
                SteamGuardOperation.StoreSecrets,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.AccountIdentifier, account.Identifier));

            return AccountSaveResult.Successful(account);
        }

        /// <inheritdoc />
        public AccountRemovalResult RemoveAccount(string accountIdentifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accountIdentifier, nameof(accountIdentifier));

            SteamAccount? account = accountStore
                .GetAll()
                .FirstOrDefault(candidate => string.Equals(candidate.Identifier, accountIdentifier, StringComparison.Ordinal));

            if (account is null)
            {
                return AccountRemovalResult.Failed("The account is not configured.");
            }

            secretStore.Remove(account.SharedSecretName);
            secretStore.Remove(account.IdentitySecretName);
            secretStore.Remove(account.RevocationCodeName);
            secretStore.Remove(account.AccessTokenName);

            accountStore.Remove(accountIdentifier);

            logger.Info(
                SteamGuardOperation.DeleteSecrets,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.AccountIdentifier, accountIdentifier));

            return AccountRemovalResult.Successful();
        }

        /// <inheritdoc />
        public AuthenticatorSecrets? GetSecrets(SteamAccount account)
        {
            ArgumentNullException.ThrowIfNull(account, nameof(account));

            string? sharedSecret = secretStore.Retrieve(account.SharedSecretName);
            string? identitySecret = secretStore.Retrieve(account.IdentitySecretName);

            if (string.IsNullOrWhiteSpace(sharedSecret) || string.IsNullOrWhiteSpace(identitySecret))
            {
                logger.Warn(
                    SteamGuardOperation.StoreSecrets,
                    OperationStatus.Failure,
                    new LogInfo(SteamGuardInfoKey.AccountIdentifier, account.Identifier));

                return null;
            }

            return new AuthenticatorSecrets
            {
                SharedSecret = sharedSecret,
                IdentitySecret = identitySecret,
                RevocationCode = secretStore.Retrieve(account.RevocationCodeName) ?? string.Empty,
                AccessToken = secretStore.Retrieve(account.AccessTokenName) ?? string.Empty
            };
        }

        /// <inheritdoc />
        public bool IsAccountConfigured(string steamIdentifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(steamIdentifier, nameof(steamIdentifier));

            return accountStore
                .GetAll()
                .Any(candidate => string.Equals(candidate.SteamIdentifier, steamIdentifier, StringComparison.Ordinal));
        }

        /// <inheritdoc />
        public AccountImportResult ImportAccount(string filePath)
        {
            AccountConfigurationReadResult readResult = configurationReader.Read(
                filePath,
                out SteamAccount? account,
                out AuthenticatorSecrets? secrets);

            if (!readResult.IsSuccessful || account is null || secrets is null)
            {
                logger.Warn(
                    SteamGuardOperation.StoreSecrets,
                    OperationStatus.Failure,
                    new LogInfo(SteamGuardInfoKey.FailureReason, readResult.FailureReason));

                return AccountImportResult.Failed(readResult.FailureReason);
            }

            AccountSaveResult saveResult = SaveAccount(account, secrets);

            if (!saveResult.IsSuccessful)
            {
                return AccountImportResult.Failed(
                    string.IsNullOrWhiteSpace(saveResult.FailureReason)
                        ? string.Join(" ", saveResult.ValidationErrors)
                        : saveResult.FailureReason);
            }

            bool isConfigurationFileRemaining = !configurationReader.Remove(filePath);

            if (isConfigurationFileRemaining)
            {
                logger.Warn(
                    SteamGuardOperation.StoreSecrets,
                    OperationStatus.Failure,
                    new LogInfo(SteamGuardInfoKey.FailureReason, "The imported configuration file could not be deleted."));
            }

            logger.Info(
                SteamGuardOperation.StoreSecrets,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.AccountIdentifier, saveResult.Account?.Identifier ?? string.Empty));

            return AccountImportResult.Successful(saveResult.Account!, isConfigurationFileRemaining);
        }

        /// <inheritdoc />
        public AccountAccessTokenResult SaveAccessToken(string steamIdentifier, string accessToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(steamIdentifier, nameof(steamIdentifier));
            ArgumentException.ThrowIfNullOrWhiteSpace(accessToken, nameof(accessToken));

            SteamAccount? account = accountStore
                .GetAll()
                .FirstOrDefault(candidate => string.Equals(candidate.SteamIdentifier, steamIdentifier, StringComparison.Ordinal));

            if (account is null)
            {
                return AccountAccessTokenResult.Failed("The account is not configured.");
            }

            if (string.IsNullOrWhiteSpace(account.AccessTokenName))
            {
                account.AccessTokenName = ComposeEntryName(account.Identifier, "access-token");
                accountStore.Save(account);
            }

            SecretStoreResult result = secretStore.Store(account.AccessTokenName, accessToken.Trim());

            if (!result.IsSuccessful)
            {
                logger.Error(
                    SteamGuardOperation.StoreSecrets,
                    OperationStatus.Failure,
                    new LogInfo(SteamGuardInfoKey.SecretStoreAvailability, result.Availability.ToString()));

                return AccountAccessTokenResult.Failed(
                    "The access token could not be stored in the platform credential store.");
            }

            logger.Info(
                SteamGuardOperation.StoreSecrets,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.AccountIdentifier, account.Identifier));

            return AccountAccessTokenResult.Successful();
        }

        /// <inheritdoc />
        public SecretStoreAvailability GetSecretStoreAvailability()
        {
            SecretStoreAvailability availability = secretStore.GetAvailability();

            logger.Debug(
                SteamGuardOperation.ReadSecretStoreStatus,
                new LogInfo(SteamGuardInfoKey.SecretStoreAvailability, availability.ToString()));

            return availability;
        }

        private static string ComposeEntryName(string accountIdentifier, string secretKind) =>
            $"{SecretEntryNamePrefix}{accountIdentifier}:{secretKind}";

        private static bool IsValidSteamIdentifier(string steamIdentifier)
        {
            if (steamIdentifier.Length != SteamIdentifierLength)
            {
                return false;
            }

            return steamIdentifier.All(char.IsAsciiDigit);
        }

        private static bool IsValidIdentitySecret(string identitySecret)
        {
            if (string.IsNullOrWhiteSpace(identitySecret))
            {
                return false;
            }

            Span<byte> decodedBytes = new byte[identitySecret.Length];

            return Convert.TryFromBase64String(identitySecret.Trim(), decodedBytes, out int writtenBytes) &&
                writtenBytes > 0;
        }

        private void StoreOptionalSecret(string entryName, string value)
        {
            if (string.IsNullOrWhiteSpace(entryName) || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            secretStore.Store(entryName, value.Trim());
        }

        private List<string> ValidateAccount(SteamAccount account, AuthenticatorSecrets secrets)
        {
            List<string> validationErrors = [];

            if (string.IsNullOrWhiteSpace(account.SteamIdentifier))
            {
                validationErrors.Add("The Steam ID 64 is required.");
            }
            else if (!IsValidSteamIdentifier(account.SteamIdentifier.Trim()))
            {
                validationErrors.Add("The Steam ID 64 must contain exactly 17 digits.");
            }
            else if (IsSteamIdentifierInUse(account))
            {
                validationErrors.Add("An account with this Steam ID 64 is already configured.");
            }

            if (!otpCodeGenerator.IsValidSharedSecret(secrets.SharedSecret))
            {
                validationErrors.Add("The shared secret must be a Base32 value.");
            }

            if (!IsValidIdentitySecret(secrets.IdentitySecret))
            {
                validationErrors.Add("The identity secret must be a Base64 value.");
            }

            return validationErrors;
        }

        private bool IsSteamIdentifierInUse(SteamAccount account) =>
            accountStore
                .GetAll()
                .Any(candidate =>
                    string.Equals(candidate.SteamIdentifier, account.SteamIdentifier.Trim(), StringComparison.Ordinal) &&
                    !string.Equals(candidate.Identifier, account.Identifier, StringComparison.Ordinal));
    }
}
