using SteamGuardDesktop.Domain.Login;

namespace SteamGuardDesktop.Service.Login
{
    /// <summary>
    /// Represents the outcome of completing a sign-in.
    /// </summary>
    public sealed class LoginCompletionResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the sign-in completed.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the tokens issued by Steam.
        /// </summary>
        public LoginTokens? Tokens { get; set; }

        /// <summary>
        /// Gets or sets the safe description of the failure.
        /// </summary>
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="tokens">The tokens issued by Steam.</param>
        /// <returns>A successful result.</returns>
        public static LoginCompletionResult Successful(LoginTokens tokens) => new()
        {
            IsSuccessful = true,
            Tokens = tokens
        };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static LoginCompletionResult Failed(string failureReason) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
