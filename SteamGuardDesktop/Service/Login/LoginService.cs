using System;

using NuciLog.Core;

using SteamGuardDesktop.Client.Steam;
using SteamGuardDesktop.Domain.Login;
using SteamGuardDesktop.Logging;

namespace SteamGuardDesktop.Service.Login
{
    /// <summary>
    /// Coordinates the Steam sign-in exchange without persisting the account password.
    /// </summary>
    public sealed class LoginService(
        ISteamAuthenticationClient authenticationClient,
        IPasswordEncryptor passwordEncryptor,
        ILogger logger) : ILoginService
    {
        /// <inheritdoc />
        public LoginStartResult BeginLogin(LoginCredentials credentials)
        {
            ArgumentNullException.ThrowIfNull(credentials, nameof(credentials));

            if (string.IsNullOrWhiteSpace(credentials.AccountName) ||
                string.IsNullOrWhiteSpace(credentials.Password))
            {
                return LoginStartResult.Failed("The account name and password are required.");
            }

            SteamClientResult<LoginRsaKey> keyResult = authenticationClient.GetPasswordRsaKey(credentials.AccountName);

            if (!keyResult.IsSuccessful || keyResult.Value is null)
            {
                return LoginStartResult.Failed(keyResult.FailureReason);
            }

            LoginRsaKey rsaKey = keyResult.Value;
            string encryptedPassword = passwordEncryptor.Encrypt(rsaKey.Modulus, rsaKey.Exponent, credentials.Password);

            SteamClientResult<LoginChallenge> challengeResult = authenticationClient.BeginCredentialSession(
                credentials,
                encryptedPassword,
                rsaKey.Timestamp);

            if (!challengeResult.IsSuccessful || challengeResult.Value is null)
            {
                logger.Warn(
                    SteamGuardOperation.BeginLogin,
                    OperationStatus.Failure,
                    new LogInfo(SteamGuardInfoKey.FailureReason, challengeResult.FailureReason));

                return LoginStartResult.Failed(challengeResult.FailureReason);
            }

            return LoginStartResult.Successful(challengeResult.Value);
        }

        /// <inheritdoc />
        public LoginGuardResult SubmitGuardCode(
            LoginChallenge challenge,
            string guardCode,
            LoginGuardType guardType)
        {
            ArgumentNullException.ThrowIfNull(challenge, nameof(challenge));

            SteamClientResult<bool> result = authenticationClient.SubmitGuardCode(challenge, guardCode, guardType);

            if (!result.IsSuccessful)
            {
                return LoginGuardResult.Failed(result.FailureReason);
            }

            return LoginGuardResult.Successful();
        }

        /// <inheritdoc />
        public LoginCompletionResult CompleteLogin(LoginChallenge challenge)
        {
            ArgumentNullException.ThrowIfNull(challenge, nameof(challenge));

            SteamClientResult<LoginTokens> result = authenticationClient.PollSession(challenge);

            if (!result.IsSuccessful || result.Value is null)
            {
                return LoginCompletionResult.Failed(result.FailureReason);
            }

            return LoginCompletionResult.Successful(result.Value);
        }
    }
}
