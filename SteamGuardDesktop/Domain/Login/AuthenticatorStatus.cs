namespace SteamGuardDesktop.Domain.Login
{
    /// <summary>
    /// Describes the authenticator that is currently attached to an account.
    /// </summary>
    public sealed class AuthenticatorStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether a mobile authenticator is attached.
        /// </summary>
        public bool HasMobileAuthenticator { get; set; }

        /// <summary>
        /// Gets or sets the authenticator type reported by Steam.
        /// </summary>
        public int AuthenticatorType { get; set; }

        /// <summary>
        /// Gets or sets the Steam Guard scheme reported by Steam.
        /// </summary>
        public int SteamGuardScheme { get; set; }

        /// <summary>
        /// Gets or sets the device identifier registered for the authenticator.
        /// </summary>
        public string DeviceIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of revocation attempts that remain.
        /// </summary>
        public int RevocationAttemptsRemaining { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the account is allowed to use an authenticator.
        /// </summary>
        public bool IsAuthenticatorAllowed { get; set; }
    }
}
