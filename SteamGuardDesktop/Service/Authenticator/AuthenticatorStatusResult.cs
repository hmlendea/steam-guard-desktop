using SteamGuardDesktop.Domain.Login;

namespace SteamGuardDesktop.Service.Authenticator
{
    /// <summary>
    /// Represents the outcome of reading the authenticator status of an account.
    /// </summary>
    public sealed class AuthenticatorStatusResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the status could be read.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the authenticator status reported by Steam.
        /// </summary>
        public AuthenticatorStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the safe description of the failure.
        /// </summary>
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="status">The authenticator status reported by Steam.</param>
        /// <returns>A successful result.</returns>
        public static AuthenticatorStatusResult Successful(AuthenticatorStatus status) => new()
        {
            IsSuccessful = true,
            Status = status
        };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static AuthenticatorStatusResult Failed(string failureReason) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
