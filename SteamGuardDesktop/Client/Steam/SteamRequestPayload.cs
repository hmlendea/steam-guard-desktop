using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Represents a request payload that is serialised before it is sent to Steam.
    /// </summary>
    /// <remarks>
    /// Property names match the field names of the Steam unified endpoints, which are consumed as protobuf
    /// JSON and therefore do not follow a generic naming policy.
    /// </remarks>
    public sealed class SteamRequestPayload
    {
        /// <summary>
        /// Gets or sets the name of the account.
        /// </summary>
        [JsonPropertyName("account_name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AccountName { get; set; }

        /// <summary>
        /// Gets or sets the RSA-encrypted, Base64-encoded password.
        /// </summary>
        [JsonPropertyName("encrypted_password")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? EncryptedPassword { get; set; }

        /// <summary>
        /// Gets or sets the timestamp that the RSA key was generated for.
        /// </summary>
        [JsonPropertyName("encryption_timestamp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? EncryptionTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the friendly name reported for this device.
        /// </summary>
        [JsonPropertyName("device_friendly_name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DeviceFriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the platform type reported to Steam.
        /// </summary>
        [JsonPropertyName("platform_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? PlatformType { get; set; }

        /// <summary>
        /// Gets or sets the persistence requested for the session.
        /// </summary>
        [JsonPropertyName("persistence")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Persistence { get; set; }

        /// <summary>
        /// Gets or sets the website identifier reported to Steam.
        /// </summary>
        [JsonPropertyName("website_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? WebsiteId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of a pending sign-in.
        /// </summary>
        [JsonPropertyName("client_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the opaque request value of a pending sign-in.
        /// </summary>
        [JsonPropertyName("request_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets or sets the 64-bit Steam identifier of the account.
        /// </summary>
        [JsonPropertyName("steamid")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? SteamIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the guard code.
        /// </summary>
        [JsonPropertyName("code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Code { get; set; }

        /// <summary>
        /// Gets or sets the guard type that produced the code.
        /// </summary>
        [JsonPropertyName("code_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? CodeType { get; set; }

        /// <summary>
        /// Gets or sets the details reported for the device.
        /// </summary>
        [JsonPropertyName("device_details")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SteamDeviceDetails? DeviceDetails { get; set; }
    }
}
