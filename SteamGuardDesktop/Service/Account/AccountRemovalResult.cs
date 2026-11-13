namespace SteamGuardDesktop.Service.Account
{
    /// <summary>
    /// Represents the outcome of removing an account.
    /// </summary>
    public sealed class AccountRemovalResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the account was removed.
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
        public static AccountRemovalResult Successful() => new() { IsSuccessful = true };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static AccountRemovalResult Failed(string failureReason) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
