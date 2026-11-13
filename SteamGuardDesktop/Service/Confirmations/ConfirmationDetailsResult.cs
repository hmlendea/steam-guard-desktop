namespace SteamGuardDesktop.Service.Confirmations
{
    /// <summary>
    /// Represents the outcome of loading the details of a pending confirmation.
    /// </summary>
    public sealed class ConfirmationDetailsResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the details could be loaded.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the safe descriptive text of the confirmation.
        /// </summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the safe description of the failure.
        /// </summary>
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="details">The safe descriptive text of the confirmation.</param>
        /// <returns>A successful result.</returns>
        public static ConfirmationDetailsResult Successful(string details) => new()
        {
            IsSuccessful = true,
            Details = details
        };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static ConfirmationDetailsResult Failed(string failureReason) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
