namespace SteamGuardDesktop.Service.Authenticator
{
    /// <summary>
    /// Represents the outcome of an authenticator operation.
    /// </summary>
    public sealed class AuthenticatorOperationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation completed.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the safe description of the failure.
        /// </summary>
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <returns>A successful result.</returns>
        public static AuthenticatorOperationResult Successful() => new() { IsSuccessful = true };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static AuthenticatorOperationResult Failed(string failureReason) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
