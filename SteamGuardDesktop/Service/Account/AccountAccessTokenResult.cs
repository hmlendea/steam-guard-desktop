namespace SteamGuardDesktop.Service.Account
{
    /// <summary>
    /// Represents the outcome of storing the access token of an account.
    /// </summary>
    public sealed class AccountAccessTokenResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the token was stored.
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
        public static AccountAccessTokenResult Successful() => new() { IsSuccessful = true };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static AccountAccessTokenResult Failed(string failureReason) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
