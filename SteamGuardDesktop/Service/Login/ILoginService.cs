using System.Collections.Generic;

using SteamGuardDesktop.Domain.Login;

namespace SteamGuardDesktop.Service.Login
{
    /// <summary>
    /// Coordinates the Steam sign-in exchange without persisting the account password.
    /// </summary>
    public interface ILoginService
    {
        /// <summary>
        /// Starts a sign-in with the supplied credentials.
        /// </summary>
        /// <param name="credentials">The credentials supplied by the user.</param>
        /// <returns>The outcome of the request.</returns>
        LoginStartResult BeginLogin(LoginCredentials credentials);

        /// <summary>
        /// Submits a guard code for the pending sign-in.
        /// </summary>
        /// <param name="challenge">The pending sign-in.</param>
        /// <param name="guardCode">The guard code.</param>
        /// <param name="guardType">The guard type that produced the code.</param>
        /// <returns>The outcome of the submission.</returns>
        LoginGuardResult SubmitGuardCode(LoginChallenge challenge, string guardCode, LoginGuardType guardType);

        /// <summary>
        /// Polls the pending sign-in until Steam issues its tokens.
        /// </summary>
        /// <param name="challenge">The pending sign-in.</param>
        /// <returns>The outcome of the request.</returns>
        LoginCompletionResult CompleteLogin(LoginChallenge challenge);
    }
}
