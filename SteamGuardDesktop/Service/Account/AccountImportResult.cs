namespace SteamGuardDesktop.Service.Account
{
    /// <summary>
    /// Represents the outcome of importing an account from a configuration file.
    /// </summary>
    public sealed class AccountImportResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the account was imported.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the imported account metadata.
        /// </summary>
        public Domain.Account.SteamAccount? Account { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the configuration file could not be deleted.
        /// </summary>
        public bool IsConfigurationFileRemaining { get; set; }

        /// <summary>
        /// Gets or sets the safe description of the failure.
        /// </summary>
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="account">The imported account metadata.</param>
        /// <param name="isConfigurationFileRemaining">A value indicating whether the configuration file could not be deleted.</param>
        /// <returns>A successful result.</returns>
        public static AccountImportResult Successful(
            Domain.Account.SteamAccount account,
            bool isConfigurationFileRemaining) => new()
        {
            IsSuccessful = true,
            Account = account,
            IsConfigurationFileRemaining = isConfigurationFileRemaining
        };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static AccountImportResult Failed(string failureReason) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
