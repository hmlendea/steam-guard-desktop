using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Domain.Authenticator;

namespace SteamGuardDesktop.DataAccess.Accounts
{
    /// <summary>
    /// Reads a JSON account configuration file.
    /// </summary>
    public sealed class JsonAccountConfigurationReader : IAccountConfigurationReader
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        /// <inheritdoc />
        public AccountConfigurationReadResult Read(
            string filePath,
            out SteamAccount? account,
            out AuthenticatorSecrets? secrets)
        {
            account = null;
            secrets = null;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return AccountConfigurationReadResult.Failed("A configuration file path is required.");
            }

            if (!File.Exists(filePath))
            {
                return AccountConfigurationReadResult.Failed("The configuration file does not exist.");
            }

            AccountConfigurationFile? configuration;

            try
            {
                configuration = JsonSerializer.Deserialize<AccountConfigurationFile>(
                    File.ReadAllText(filePath),
                    SerializerOptions);
            }
            catch (JsonException)
            {
                return AccountConfigurationReadResult.Failed("The configuration file is not valid JSON.");
            }
            catch (IOException)
            {
                return AccountConfigurationReadResult.Failed("The configuration file could not be read.");
            }

            if (configuration is null)
            {
                return AccountConfigurationReadResult.Failed("The configuration file is empty.");
            }

            if (string.IsNullOrWhiteSpace(configuration.SteamIdentifier))
            {
                return AccountConfigurationReadResult.Failed("The configuration file does not contain a Steam ID 64.");
            }

            account = new SteamAccount
            {
                SteamIdentifier = configuration.SteamIdentifier.Trim(),
                Label = configuration.Label.Trim()
            };

            secrets = new AuthenticatorSecrets
            {
                SharedSecret = configuration.SharedSecret.Trim(),
                IdentitySecret = configuration.IdentitySecret.Trim(),
                RevocationCode = configuration.RevocationCode.Trim(),
                AccessToken = configuration.AccessToken.Trim()
            };

            return AccountConfigurationReadResult.Successful();
        }

        /// <inheritdoc />
        public bool Remove(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return true;
            }

            try
            {
                File.Delete(filePath);

                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
