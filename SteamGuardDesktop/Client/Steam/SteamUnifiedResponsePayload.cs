using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Represents the payload carried by a unified Steam endpoint response.
    /// </summary>
    public sealed class SteamUnifiedResponsePayload
    {
        /// <summary>
        /// Gets or sets the RSA public key modulus.
        /// </summary>
        [JsonPropertyName("publickey_mod")]
        public string? PublicKeyModulus { get; set; }

        /// <summary>
        /// Gets or sets the RSA public key exponent.
        /// </summary>
        [JsonPropertyName("publickey_exp")]
        public string? PublicKeyExponent { get; set; }

        /// <summary>
        /// Gets or sets the timestamp for which the RSA key was generated.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public long? RsaTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the identifier that Steam uses to route the authentication attempt.
        /// </summary>
        [JsonPropertyName("client_id")]
        public string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the opaque request value returned when the attempt starts.
        /// </summary>
        [JsonPropertyName("request_id")]
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets or sets the interval, in seconds, at which Steam expects the attempt to be polled.
        /// </summary>
        [JsonPropertyName("interval")]
        public double? PollingIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets the 64-bit Steam identifier.
        /// </summary>
        [JsonPropertyName("steamid")]
        public string? SteamIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the guard-specific message reported by Steam.
        /// </summary>
        [JsonPropertyName("extended_error_message")]
        public string? ExtendedErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the guards that Steam accepts.
        /// </summary>
        [JsonPropertyName("allowed_confirmations")]
        public SteamAllowedConfirmation[] AllowedConfirmations { get; set; } = [];

        /// <summary>
        /// Gets or sets the access token issued after a successful sign-in.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the refresh token issued after a successful sign-in.
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the shared secret of a newly enrolled authenticator.
        /// </summary>
        [JsonPropertyName("shared_secret")]
        public byte[]? SharedSecret { get; set; }

        /// <summary>
        /// Gets or sets the identity secret of a newly enrolled authenticator.
        /// </summary>
        [JsonPropertyName("identity_secret")]
        public byte[]? IdentitySecret { get; set; }

        /// <summary>
        /// Gets or sets the revocation code of a newly enrolled authenticator.
        /// </summary>
        [JsonPropertyName("revocation_code")]
        public string? RevocationCode { get; set; }

        /// <summary>
        /// Gets or sets the device identifier that Steam assigned to the authenticator.
        /// </summary>
        [JsonPropertyName("device_identifier")]
        public string? DeviceIdentifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an operation succeeded.
        /// </summary>
        [JsonPropertyName("success")]
        public bool? IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Steam requires another activation step.
        /// </summary>
        [JsonPropertyName("want_more")]
        public bool? WantsMore { get; set; }

        /// <summary>
        /// Gets or sets the authenticator type reported by Steam.
        /// </summary>
        [JsonPropertyName("authenticator_type")]
        public int? AuthenticatorType { get; set; }

        /// <summary>
        /// Gets or sets the Steam Guard scheme reported by Steam.
        /// </summary>
        [JsonPropertyName("steamguard_scheme")]
        public int? SteamGuardScheme { get; set; }

        /// <summary>
        /// Gets or sets the number of revocation attempts that remain.
        /// </summary>
        [JsonPropertyName("revocation_attempts_remaining")]
        public int? RevocationAttemptsRemaining { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the account may use an authenticator.
        /// </summary>
        [JsonPropertyName("authenticator_allowed")]
        public bool? IsAuthenticatorAllowed { get; set; }

        /// <summary>
        /// Gets or sets the protocol status reported for an authenticator operation.
        /// </summary>
        [JsonPropertyName("status")]
        public int? Status { get; set; }

        /// <summary>
        /// Gets or sets the masked phone number that Steam associates with the authenticator.
        /// </summary>
        [JsonPropertyName("phone_number_hint")]
        public string? PhoneNumberHint { get; set; }
    }
}
