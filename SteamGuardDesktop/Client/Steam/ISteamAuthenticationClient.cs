using System.Collections.Generic;

using SteamGuardDesktop.Domain.Login;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Communicates with the Steam authentication and two-factor endpoints.
    /// </summary>
    public interface ISteamAuthenticationClient
    {
        /// <summary>
        /// Retrieves the RSA public key that Steam publishes for an account password exchange.
        /// </summary>
        /// <param name="accountName">The account name.</param>
        /// <returns>The RSA public key.</returns>
        SteamClientResult<LoginRsaKey> GetPasswordRsaKey(string accountName);

        /// <summary>
        /// Starts a credential sign-in.
        /// </summary>
        /// <param name="credentials">The credentials supplied by the user.</param>
        /// <param name="encryptedPassword">The RSA-encrypted, Base64-encoded password.</param>
        /// <param name="encryptionTimestamp">The timestamp that the RSA key was generated for.</param>
        /// <returns>The pending sign-in that Steam is waiting to be confirmed.</returns>
        SteamClientResult<LoginChallenge> BeginCredentialSession(
            LoginCredentials credentials,
            string encryptedPassword,
            long encryptionTimestamp);

        /// <summary>
        /// Submits a guard code for a pending sign-in.
        /// </summary>
        /// <param name="challenge">The pending sign-in.</param>
        /// <param name="guardCode">The guard code.</param>
        /// <param name="guardType">The guard type that produced the code.</param>
        /// <returns>The outcome of the submission.</returns>
        SteamClientResult<bool> SubmitGuardCode(LoginChallenge challenge, string guardCode, LoginGuardType guardType);

        /// <summary>
        /// Polls a pending sign-in until Steam issues its tokens.
        /// </summary>
        /// <param name="challenge">The pending sign-in.</param>
        /// <returns>The issued tokens.</returns>
        SteamClientResult<LoginTokens> PollSession(LoginChallenge challenge);

        /// <summary>
        /// Retrieves the authenticator status of an account.
        /// </summary>
        /// <param name="accessToken">The access token of the authenticated session.</param>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <returns>The authenticator status.</returns>
        SteamClientResult<AuthenticatorStatus> GetAuthenticatorStatus(string accessToken, string steamIdentifier);

        /// <summary>
        /// Removes the authenticator of an account with its revocation code.
        /// </summary>
        /// <param name="accessToken">The access token of the authenticated session.</param>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <param name="revocationCode">The revocation code of the authenticator.</param>
        /// <returns>The outcome of the removal.</returns>
        SteamClientResult<bool> RemoveAuthenticator(
            string accessToken,
            string steamIdentifier,
            string revocationCode);

        /// <summary>
        /// Enrols a new authenticator for an account.
        /// </summary>
        /// <param name="accessToken">The access token of the authenticated session.</param>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <param name="deviceIdentifier">The device identifier to report.</param>
        /// <returns>The enrolled authenticator.</returns>
        SteamClientResult<EnrolledAuthenticator> AddAuthenticator(
            string accessToken,
            string steamIdentifier,
            string deviceIdentifier);

        /// <summary>
        /// Completes the enrolment of an authenticator with an activation code.
        /// </summary>
        /// <param name="accessToken">The access token of the authenticated session.</param>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <param name="activationCode">The activation code issued by Steam.</param>
        /// <param name="deviceIdentifier">The device identifier to report.</param>
        /// <returns>The outcome of the finalisation.</returns>
        SteamClientResult<bool> FinalizeAuthenticator(
            string accessToken,
            string steamIdentifier,
            string activationCode,
            string deviceIdentifier);

        /// <summary>
        /// Starts the revocation of an authenticator whose device is no longer available.
        /// </summary>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <param name="accessToken">The access token of the authenticated session.</param>
        /// <returns>The outcome of the request.</returns>
        SteamClientResult<bool> StartAuthenticatorRevocation(string steamIdentifier, string accessToken);

        /// <summary>
        /// Continues the revocation of an authenticator whose device is no longer available.
        /// </summary>
        /// <param name="accessToken">The access token of the authenticated session.</param>
        /// <param name="smsCode">The code sent to the registered phone number.</param>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <returns>The replacement authenticator issued by Steam.</returns>
        SteamClientResult<EnrolledAuthenticator> ContinueAuthenticatorRevocation(
            string accessToken,
            string smsCode,
            string steamIdentifier);

        /// <summary>
        /// Signs every other Steam session out of the account.
        /// </summary>
        /// <param name="accessToken">The access token of the authenticated session.</param>
        /// <returns>The outcome of the request.</returns>
        SteamClientResult<bool> RemoveAllGuardCookies(string accessToken);
    }
}
