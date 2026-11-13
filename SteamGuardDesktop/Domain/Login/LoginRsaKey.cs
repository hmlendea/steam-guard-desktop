namespace SteamGuardDesktop.Domain.Login
{
    /// <summary>
    /// Represents the RSA public key that Steam publishes for a password exchange.
    /// </summary>
    public sealed class LoginRsaKey
    {
        /// <summary>
        /// Gets or sets the RSA public key modulus.
        /// </summary>
        public string Modulus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the RSA public key exponent.
        /// </summary>
        public string Exponent { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp for which the key was generated.
        /// </summary>
        public long Timestamp { get; set; }
    }
}
