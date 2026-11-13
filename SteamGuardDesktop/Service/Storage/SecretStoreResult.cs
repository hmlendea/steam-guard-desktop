namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Represents the outcome of a secret store operation.
    /// </summary>
    public sealed class SecretStoreResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation completed successfully.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the availability of the credential store after the operation.
        /// </summary>
        public SecretStoreAvailability Availability { get; set; } = SecretStoreAvailability.Available;

        /// <summary>
        /// Gets or sets the safe description of the failure, when the operation did not succeed.
        /// </summary>
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <returns>A successful result.</returns>
        public static SecretStoreResult Successful() => new()
        {
            IsSuccessful = true,
            Availability = SecretStoreAvailability.Available
        };

        /// <summary>
        /// Creates a failed result for the specified availability.
        /// </summary>
        /// <param name="availability">The availability observed while failing.</param>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static SecretStoreResult Failed(SecretStoreAvailability availability, string failureReason) => new()
        {
            IsSuccessful = false,
            Availability = availability,
            FailureReason = failureReason
        };
    }
}
