using System.Text.Json.Serialization;

namespace SteamGuardDesktop.DataAccess.Accounts
{
    /// <summary>
    /// Represents an account configuration file that can be imported.
    /// </summary>
    /// <remarks>
    /// The file carries secrets in clear text and is expected to be deleted by the user after the import,
    /// because the application stores every secret in the platform credential store or the encrypted fallback.
    /// </remarks>
    public sealed class AccountConfigurationFile
    {
        /// <summary>
        /// Gets or sets the 64-bit Steam identifier of the account.
        /// </summary>
        [JsonPropertyName("steamid")]
        public string SteamIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user-facing account label.
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Base32 shared secret.
        /// </summary>
        [JsonPropertyName("shared_secret")]
        public string SharedSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Base64 identity secret.
        /// </summary>
        [JsonPropertyName("identity_secret")]
        public string IdentitySecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the revocation code.
        /// </summary>
        [JsonPropertyName("revocation_code")]
        public string RevocationCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Steam access token.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }
}
