using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Serialises the request payloads sent to the Steam unified endpoints.
    /// </summary>
    public sealed class SteamRequestPayloadBuilder : ISteamRequestPayloadBuilder
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static int SteamClientPlatformType => 3;

        private static int PersistentSession => 1;

        private static string WebDomainIdentifier => "Community";

        private static int WindowsOsType => 16;

        private static int DesktopGamingDeviceType => 1;

        /// <inheritdoc />
        public string BuildPasswordRsaKeyPayload(string accountName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accountName, nameof(accountName));

            SteamRequestPayload payload = new() { AccountName = accountName };

            return Serialize(payload);
        }

        /// <inheritdoc />
        public string BuildBeginCredentialSessionPayload(
            string accountName,
            string encryptedPassword,
            long encryptionTimestamp,
            string deviceFriendlyName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accountName, nameof(accountName));
            ArgumentException.ThrowIfNullOrWhiteSpace(encryptedPassword, nameof(encryptedPassword));

            SteamRequestPayload payload = new()
            {
                AccountName = accountName,
                EncryptedPassword = encryptedPassword,
                EncryptionTimestamp = encryptionTimestamp,
                DeviceFriendlyName = deviceFriendlyName,
                PlatformType = SteamClientPlatformType,
                Persistence = PersistentSession,
                WebsiteId = WebDomainIdentifier,
                DeviceDetails = new SteamDeviceDetails
                {
                    DeviceFriendlyName = deviceFriendlyName,
                    PlatformType = SteamClientPlatformType,
                    OsType = WindowsOsType,
                    GamingDeviceType = DesktopGamingDeviceType
                }
            };

            return Serialize(payload);
        }

        /// <inheritdoc />
        public string BuildGuardCodePayload(
            string clientId,
            string steamIdentifier,
            string guardCode,
            int guardType)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(clientId, nameof(clientId));
            ArgumentException.ThrowIfNullOrWhiteSpace(guardCode, nameof(guardCode));

            SteamRequestPayload payload = new()
            {
                ClientId = ParseIdentifier(clientId),
                SteamIdentifier = ParseIdentifier(steamIdentifier),
                Code = guardCode,
                CodeType = guardType
            };

            return Serialize(payload);
        }

        /// <inheritdoc />
        public string BuildPollSessionPayload(string clientId, string requestId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(clientId, nameof(clientId));
            ArgumentException.ThrowIfNullOrWhiteSpace(requestId, nameof(requestId));

            SteamRequestPayload payload = new()
            {
                ClientId = ParseIdentifier(clientId),
                RequestId = requestId
            };

            return Serialize(payload);
        }

        private static string Serialize(SteamRequestPayload payload)
            => JsonSerializer.Serialize(payload, SerializerOptions);

        private static long? ParseIdentifier(string identifier)
        {
            if (long.TryParse(identifier, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsedIdentifier))
            {
                return parsedIdentifier;
            }

            return null;
        }
    }
}
