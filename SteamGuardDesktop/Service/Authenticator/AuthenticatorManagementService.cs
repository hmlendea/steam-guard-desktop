using System;

using NuciLog.Core;

using SteamGuardDesktop.Client.Steam;
using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Domain.Login;
using SteamGuardDesktop.Logging;

namespace SteamGuardDesktop.Service.Authenticator
{
    /// <summary>
    /// Coordinates the enrolment, inspection, removal, and recovery of a Steam Mobile Authenticator.
    /// </summary>
    public sealed class AuthenticatorManagementService(
        ISteamAuthenticationClient authenticationClient,
        IDeviceIdentityProvider deviceIdentityProvider,
        IOtpCodeGenerator otpCodeGenerator,
        ILogger logger) : IAuthenticatorManagementService
    {
        /// <inheritdoc />
        public AuthenticatorStatusResult GetStatus(string steamIdentifier, string accessToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(steamIdentifier, nameof(steamIdentifier));
            ArgumentException.ThrowIfNullOrWhiteSpace(accessToken, nameof(accessToken));

            SteamClientResult<AuthenticatorStatus> result = authenticationClient.GetAuthenticatorStatus(
                accessToken,
                steamIdentifier);

            if (!result.IsSuccessful || result.Value is null)
            {
                return AuthenticatorStatusResult.Failed(result.FailureReason);
            }

            logger.Info(
                SteamGuardOperation.ReadAuthenticatorStatus,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.HasMobileAuthenticator, result.Value.HasMobileAuthenticator));

            return AuthenticatorStatusResult.Successful(result.Value);
        }

        /// <inheritdoc />
        public AuthenticatorOperationResult RemoveAuthenticator(
            string steamIdentifier,
            string accessToken,
            string revocationCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(revocationCode, nameof(revocationCode));

            SteamClientResult<bool> result = authenticationClient.RemoveAuthenticator(
                accessToken,
                steamIdentifier,
                revocationCode);

            if (!result.IsSuccessful)
            {
                return AuthenticatorOperationResult.Failed(result.FailureReason);
            }

            logger.Info(
                SteamGuardOperation.RemoveAuthenticator,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.AccountIdentifier, steamIdentifier));

            return AuthenticatorOperationResult.Successful();
        }

        /// <inheritdoc />
        public AuthenticatorEnrolmentResult AddAuthenticator(string steamIdentifier, string accessToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(steamIdentifier, nameof(steamIdentifier));
            ArgumentException.ThrowIfNullOrWhiteSpace(accessToken, nameof(accessToken));

            string deviceIdentifier = deviceIdentityProvider.DeriveDeviceIdentifier(steamIdentifier);

            SteamClientResult<EnrolledAuthenticator> result = authenticationClient.AddAuthenticator(
                accessToken,
                steamIdentifier,
                deviceIdentifier);

            if (!result.IsSuccessful || result.Value is null)
            {
                return AuthenticatorEnrolmentResult.Failed(result.FailureReason);
            }

            string? activationCode = GenerateActivationCode(result.Value);

            if (activationCode is null)
            {
                return AuthenticatorEnrolmentResult.Failed("The authenticator shared secret could not be used.");
            }

            return AuthenticatorEnrolmentResult.Successful(result.Value, activationCode);
        }

        /// <inheritdoc />
        public AuthenticatorOperationResult FinalizeAuthenticator(
            string steamIdentifier,
            string accessToken,
            string activationCode,
            string deviceIdentifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(activationCode, nameof(activationCode));

            SteamClientResult<bool> result = authenticationClient.FinalizeAuthenticator(
                accessToken,
                steamIdentifier,
                activationCode,
                deviceIdentifier);

            if (!result.IsSuccessful)
            {
                return AuthenticatorOperationResult.Failed(result.FailureReason);
            }

            logger.Info(
                SteamGuardOperation.AddAuthenticator,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.AccountIdentifier, steamIdentifier));

            return AuthenticatorOperationResult.Successful();
        }

        /// <inheritdoc />
        public AuthenticatorEnrolmentResult RecoverAuthenticator(
            string steamIdentifier,
            string accessToken,
            string smsCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(smsCode, nameof(smsCode));

            SteamClientResult<EnrolledAuthenticator> result = authenticationClient.ContinueAuthenticatorRevocation(
                accessToken,
                smsCode,
                steamIdentifier);

            if (!result.IsSuccessful || result.Value is null)
            {
                return AuthenticatorEnrolmentResult.Failed(result.FailureReason);
            }

            return AuthenticatorEnrolmentResult.Successful(result.Value, string.Empty);
        }

        /// <inheritdoc />
        public AuthenticatorSecrets CreateSecrets(EnrolledAuthenticator enrolledAuthenticator)
        {
            ArgumentNullException.ThrowIfNull(enrolledAuthenticator, nameof(enrolledAuthenticator));

            return new AuthenticatorSecrets
            {
                SharedSecret = enrolledAuthenticator.SharedSecret,
                IdentitySecret = enrolledAuthenticator.IdentitySecret,
                RevocationCode = enrolledAuthenticator.RevocationCode
            };
        }

        /// <inheritdoc />
        public string? GenerateActivationCode(EnrolledAuthenticator enrolledAuthenticator)
        {
            ArgumentNullException.ThrowIfNull(enrolledAuthenticator, nameof(enrolledAuthenticator));

            return otpCodeGenerator.GenerateCode(enrolledAuthenticator.SharedSecret)?.Value;
        }
    }
}
