using SteamGuardDesktop.Domain.Login;

namespace SteamGuardDesktop.Service.Authenticator
{
    /// <summary>
    /// Represents the outcome of enrolling or recovering an authenticator.
    /// </summary>
    public sealed class AuthenticatorEnrolmentResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether an authenticator was issued.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the authenticator issued by Steam.
        /// </summary>
        public EnrolledAuthenticator? Authenticator { get; set; }

        /// <summary>
        /// Gets or sets the safe description of the failure.
        /// </summary>
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="authenticator">The authenticator issued by Steam.</param>
        /// <returns>A successful result.</returns>
        public static AuthenticatorEnrolmentResult Successful(EnrolledAuthenticator authenticator) => new()
        {
            IsSuccessful = true,
            Authenticator = authenticator
        };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static AuthenticatorEnrolmentResult Failed(string failureReason) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
