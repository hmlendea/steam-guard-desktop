namespace SteamGuardDesktop.DataAccess.Accounts
{
    /// <summary>
    /// Represents the outcome of reading an account configuration file.
    /// </summary>
    public sealed class AccountConfigurationReadResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the file was read.
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
        public static AccountConfigurationReadResult Successful() => new() { IsSuccessful = true };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static AccountConfigurationReadResult Failed(string failureReason) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
