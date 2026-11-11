namespace SteamGuardDesktop.Service.Confirmations
{
    /// <summary>
    /// Represents the outcome of applying a decision to a pending confirmation.
    /// </summary>
    public sealed class ConfirmationDecisionResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the decision was applied.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a new Steam session is required.
        /// </summary>
        public bool RequiresAuthentication { get; set; }

        /// <summary>
        /// Gets or sets the safe description of the failure.
        /// </summary>
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <returns>A successful result.</returns>
        public static ConfirmationDecisionResult Successful() => new() { IsSuccessful = true };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <param name="requiresAuthentication">A value indicating whether a new Steam session is required.</param>
        /// <returns>A failed result.</returns>
        public static ConfirmationDecisionResult Failed(string failureReason, bool requiresAuthentication) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason,
            RequiresAuthentication = requiresAuthentication
        };
    }
}
