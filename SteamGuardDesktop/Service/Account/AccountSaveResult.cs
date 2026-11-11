using System.Collections.Generic;

namespace SteamGuardDesktop.Service.Account
{
    /// <summary>
    /// Represents the outcome of saving an account.
    /// </summary>
    public sealed class AccountSaveResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the account was saved.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the saved account.
        /// </summary>
        public Domain.Account.SteamAccount? Account { get; set; }

        /// <summary>
        /// Gets or sets the safe explanation of each invalid input field.
        /// </summary>
        public IEnumerable<string> ValidationErrors { get; set; } = [];

        /// <summary>
        /// Gets or sets the safe description of the failure.
        /// </summary>
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="account">The saved account.</param>
        /// <returns>A successful result.</returns>
        public static AccountSaveResult Successful(Domain.Account.SteamAccount account) => new()
        {
            IsSuccessful = true,
            Account = account
        };

        /// <summary>
        /// Creates a result that reports invalid input.
        /// </summary>
        /// <param name="validationErrors">The safe explanation of each invalid input field.</param>
        /// <returns>An invalid result.</returns>
        public static AccountSaveResult Invalid(IEnumerable<string> validationErrors) => new()
        {
            IsSuccessful = false,
            ValidationErrors = validationErrors
        };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static AccountSaveResult Failed(string failureReason) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
