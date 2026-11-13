namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Represents the outcome of a Steam web request.
    /// </summary>
    /// <typeparam name="TValue">The type of the value produced by the request.</typeparam>
    public sealed class SteamClientResult<TValue>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the request completed successfully.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the value produced by the request.
        /// </summary>
        public TValue? Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Steam rejected the session.
        /// </summary>
        public bool RequiresAuthentication { get; set; }

        /// <summary>
        /// Gets or sets the safe description of the failure.
        /// </summary>
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="value">The value produced by the request.</param>
        /// <returns>A successful result.</returns>
        public static SteamClientResult<TValue> Successful(TValue value) => new()
        {
            IsSuccessful = true,
            Value = value
        };

        /// <summary>
        /// Creates a failed result for a rejected session.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result that requires authentication.</returns>
        public static SteamClientResult<TValue> AuthenticationRequired(string failureReason) => new()
        {
            IsSuccessful = false,
            RequiresAuthentication = true,
            FailureReason = failureReason
        };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static SteamClientResult<TValue> Failed(string failureReason) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
