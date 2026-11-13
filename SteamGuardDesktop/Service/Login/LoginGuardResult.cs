namespace SteamGuardDesktop.Service.Login
{
    /// <summary>
    /// Represents the outcome of submitting a guard code.
    /// </summary>
    public sealed class LoginGuardResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the guard code was accepted.
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
        public static LoginGuardResult Successful() => new() { IsSuccessful = true };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static LoginGuardResult Failed(string failureReason) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
