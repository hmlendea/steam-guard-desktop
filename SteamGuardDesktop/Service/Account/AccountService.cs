using System;
using System.Collections.Generic;
using System.Linq;

using SteamGuardDesktop.DataAccess.Accounts;
using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Domain.Authenticator;
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
        IOtpCodeGenerator otpCodeGenerator) : IAccountService
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

                return AccountSaveResult.Failed("The secrets could not be stored in the platform credential store.");
            }

            if (!string.IsNullOrWhiteSpace(secrets.RevocationCode))
            {
                secretStore.Store(account.RevocationCodeName, secrets.RevocationCode.Trim());
            }

            if (!string.IsNullOrWhiteSpace(secrets.AccessToken))
            {
                secretStore.Store(account.AccessTokenName, secrets.AccessToken.Trim());
            }

            accountStore.Save(account);

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
        public SecretStoreAvailability GetSecretStoreAvailability() => secretStore.GetAvailability();

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

        private static string ComposeEntryName(string accountIdentifier, string secretKind) =>
            $"{SecretEntryNamePrefix}{accountIdentifier}:{secretKind}";
    }
}
