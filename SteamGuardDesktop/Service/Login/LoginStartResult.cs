using System.Collections.Generic;

using SteamGuardDesktop.Domain.Login;

namespace SteamGuardDesktop.Service.Login
{
    /// <summary>
    /// Represents the outcome of starting a sign-in.
    /// </summary>
    public sealed class LoginStartResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the sign-in was started.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the pending sign-in that Steam is waiting to be confirmed.
        /// </summary>
        public LoginChallenge? Challenge { get; set; }

        /// <summary>
        /// Gets or sets the safe description of the failure.
        /// </summary>
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="challenge">The pending sign-in.</param>
        /// <returns>A successful result.</returns>
        public static LoginStartResult Successful(LoginChallenge challenge) => new()
        {
            IsSuccessful = true,
            Challenge = challenge
        };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="failureReason">The safe description of the failure.</param>
        /// <returns>A failed result.</returns>
        public static LoginStartResult Failed(string failureReason) => new()
        {
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
