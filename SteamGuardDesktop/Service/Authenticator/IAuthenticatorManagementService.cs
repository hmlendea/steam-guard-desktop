using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Domain.Login;

namespace SteamGuardDesktop.Service.Authenticator
{
    /// <summary>
    /// Coordinates the enrolment, inspection, removal, and recovery of a Steam Mobile Authenticator.
    /// </summary>
    public interface IAuthenticatorManagementService
    {
        /// <summary>
        /// Reports the authenticator status of an account.
        /// </summary>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <param name="accessToken">The access token of the authenticated session.</param>
        /// <returns>The outcome of the operation.</returns>
        AuthenticatorStatusResult GetStatus(string steamIdentifier, string accessToken);

        /// <summary>
        /// Removes the authenticator of an account with its revocation code.
        /// </summary>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <param name="accessToken">The access token of the authenticated session.</param>
        /// <param name="revocationCode">The revocation code of the authenticator.</param>
        /// <returns>The outcome of the operation.</returns>
        AuthenticatorOperationResult RemoveAuthenticator(
            string steamIdentifier,
            string accessToken,
            string revocationCode);

        /// <summary>
        /// Enrols a new authenticator and returns the secrets that must be activated.
        /// </summary>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <param name="accessToken">The access token of the authenticated session.</param>
        /// <returns>The outcome of the operation.</returns>
        AuthenticatorEnrolmentResult AddAuthenticator(string steamIdentifier, string accessToken);

        /// <summary>
        /// Completes an enrolment with the activation code derived from the shared secret.
        /// </summary>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <param name="accessToken">The access token of the authenticated session.</param>
        /// <param name="enrolledAuthenticator">The authenticator issued by Steam.</param>
        /// <param name="smsActivationCode">The code that Steam sent to the registered phone number.</param>
        /// <returns>The outcome of the operation.</returns>
        AuthenticatorOperationResult FinalizeAuthenticator(
            string steamIdentifier,
            string accessToken,
            EnrolledAuthenticator enrolledAuthenticator,
            string smsActivationCode);

        /// <summary>
        /// Recovers an account whose authenticator device is no longer available.
        /// </summary>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <param name="accessToken">The access token of the authenticated session.</param>
        /// <param name="smsCode">The code sent to the registered phone number.</param>
        /// <returns>The outcome of the operation.</returns>
        AuthenticatorEnrolmentResult RecoverAuthenticator(
            string steamIdentifier,
            string accessToken,
            string smsCode);

        /// <summary>
        /// Signs every other Steam session out of the account.
        /// </summary>
        /// <param name="accessToken">The access token of the authenticated session.</param>
        /// <returns>The outcome of the operation.</returns>
        AuthenticatorOperationResult RemoveAllGuardCookies(string accessToken);

        /// <summary>
        /// Creates the secrets that a recovered authenticator should use.
        /// </summary>
        /// <param name="enrolledAuthenticator">The authenticator issued by Steam.</param>
        /// <returns>The secrets to store for the account.</returns>
        AuthenticatorSecrets CreateSecrets(EnrolledAuthenticator enrolledAuthenticator);
    }
}
