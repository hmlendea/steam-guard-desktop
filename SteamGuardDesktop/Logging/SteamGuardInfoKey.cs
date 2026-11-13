using NuciLog.Core;

namespace SteamGuardDesktop.Logging
{
    /// <summary>
    /// Defines the structured fields reported to the diagnostics log.
    /// </summary>
    public sealed class SteamGuardInfoKey : LogInfoKey
    {
        /// <summary>
        /// Gets the structured field that identifies the account.
        /// </summary>
        public static LogInfoKey AccountIdentifier => new SteamGuardInfoKey(nameof(AccountIdentifier));

        /// <summary>
        /// Gets the structured field that reports the number of configured accounts.
        /// </summary>
        public static LogInfoKey AccountCount => new SteamGuardInfoKey(nameof(AccountCount));

        /// <summary>
        /// Gets the structured field that reports the credential store availability.
        /// </summary>
        public static LogInfoKey SecretStoreAvailability => new SteamGuardInfoKey(nameof(SecretStoreAvailability));

        /// <summary>
        /// Gets the structured field that records a failure reason.
        /// </summary>
        public static LogInfoKey FailureReason => new SteamGuardInfoKey(nameof(FailureReason));

        /// <summary>
        /// Gets the structured field that reports the resolved platform theme.
        /// </summary>
        public static LogInfoKey PlatformTheme => new SteamGuardInfoKey(nameof(PlatformTheme));

        /// <summary>
        /// Gets the structured field that reports the detected desktop environment.
        /// </summary>
        public static LogInfoKey DesktopEnvironment => new SteamGuardInfoKey(nameof(DesktopEnvironment));

        /// <summary>
        /// Gets the structured field that records an HTTP status code.
        /// </summary>
        public static LogInfoKey HttpStatusCode => new SteamGuardInfoKey(nameof(HttpStatusCode));

        /// <summary>
        /// Gets the structured field that records the requested Steam operation.
        /// </summary>
        public static LogInfoKey SteamOperation => new SteamGuardInfoKey(nameof(SteamOperation));

        /// <summary>
        /// Gets the structured field that reports the guard accepted for a sign-in.
        /// </summary>
        public static LogInfoKey LoginGuard => new SteamGuardInfoKey(nameof(LoginGuard));

        /// <summary>
        /// Gets the structured field that reports whether an authenticator is attached.
        /// </summary>
        public static LogInfoKey HasMobileAuthenticator => new SteamGuardInfoKey(nameof(HasMobileAuthenticator));

        /// <summary>
        /// Gets the structured field that reports the number of pending confirmations.
        /// </summary>
        public static LogInfoKey ConfirmationCount => new SteamGuardInfoKey(nameof(ConfirmationCount));

        /// <summary>
        /// Gets the sensitive structured field that records a secret value. Its value is always masked.
        /// </summary>
        public static LogInfoKey Secret => new SteamGuardInfoKey(nameof(Secret), true);

        /// <summary>
        /// Gets the sensitive structured field that records a session access token. Its value is always masked.
        /// </summary>
        public static LogInfoKey AccessToken => new SteamGuardInfoKey(nameof(AccessToken), true);

        /// <summary>
        /// Gets the sensitive structured field that records a request signature. Its value is always masked.
        /// </summary>
        public static LogInfoKey Signature => new SteamGuardInfoKey(nameof(Signature), true);

        /// <summary>
        /// Gets the sensitive structured field that records a request address. Its value is always masked.
        /// </summary>
        public static LogInfoKey RequestAddress => new SteamGuardInfoKey(nameof(RequestAddress), true);

        private SteamGuardInfoKey(string name)
            : base(name)
        {
        }

        private SteamGuardInfoKey(string name, bool isSensitive)
            : base(name, isSensitive)
        {
        }
    }
}
