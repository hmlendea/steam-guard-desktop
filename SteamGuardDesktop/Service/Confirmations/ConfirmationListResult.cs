using System.Collections.Generic;

using SteamGuardDesktop.Domain.Confirmations;

namespace SteamGuardDesktop.Service.Confirmations
{
    /// <summary>
    /// Represents the outcome of loading the pending confirmations of an account.
    /// </summary>
    public sealed class ConfirmationListResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the confirmations could be loaded.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the pending confirmations.
        /// </summary>
        public IEnumerable<Confirmation> Confirmations { get; set; } = [];

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
        /// <param name="confirmations">The pending confirmations.</param>
        /// <returns>A successful result.</returns>
        public static ConfirmationListResult Successful(IEnumerable<Confirmation> confirmations) => new()
        {
            IsSuccessful = true,
            Confirmations = confirmations
        };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <param name="requiresAuthentication">A value indicating whether a new Steam session is required.</param>
        /// <returns>A failed result.</returns>
        public static ConfirmationListResult Failed(string failureReason, bool requiresAuthentication) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason,
            RequiresAuthentication = requiresAuthentication
        };
    }
}
