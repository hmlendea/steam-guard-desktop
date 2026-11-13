using System;
using System.Threading;

using NuciLog.Core;

using SteamGuardDesktop.Client.Steam;
using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Domain.Login;
using SteamGuardDesktop.Logging;
using SteamGuardDesktop.Service.Time;

namespace SteamGuardDesktop.Service.Authenticator
{
    /// <summary>
    /// Coordinates the enrolment, inspection, removal, and recovery of a Steam Mobile Authenticator.
    /// </summary>
    public sealed class AuthenticatorManagementService(
        ISteamAuthenticationClient authenticationClient,
        IDeviceIdentityProvider deviceIdentityProvider,
        IOtpCodeGenerator otpCodeGenerator,
        IClockDriftService clockDriftService,
        ILogger logger) : IAuthenticatorManagementService
    {
        private static int MaximumFinalizationAttempts => 30;

        private static TimeSpan FinalizationRetryDelay => TimeSpan.FromSeconds(5);

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

            logger.Info(
                SteamGuardOperation.AddAuthenticator,
                OperationStatus.Started,
                new LogInfo(SteamGuardInfoKey.AccountIdentifier, steamIdentifier));

            return AuthenticatorEnrolmentResult.Successful(result.Value);
        }

        /// <inheritdoc />
        public AuthenticatorOperationResult FinalizeAuthenticator(
            string steamIdentifier,
            string accessToken,
            EnrolledAuthenticator enrolledAuthenticator,
            string smsActivationCode)
        {
            ArgumentNullException.ThrowIfNull(enrolledAuthenticator, nameof(enrolledAuthenticator));
            ArgumentException.ThrowIfNullOrWhiteSpace(smsActivationCode, nameof(smsActivationCode));

            ClockDriftReport? driftReport = clockDriftService.Evaluate();

            if (driftReport is null)
            {
                return AuthenticatorOperationResult.Failed(
                    "The Steam clock could not be read, so the authenticator cannot be activated safely.");
            }

            if (driftReport.RequiresAttention)
            {
                return AuthenticatorOperationResult.Failed(
                    "The local clock differs from the Steam clock by more than the accepted window. " +
                    "Synchronise the operating system clock and activate the authenticator again.");
            }

            for (int attempt = 0; attempt < MaximumFinalizationAttempts; ++attempt)
            {
                string? activationCode = GenerateActivationCode(enrolledAuthenticator);

                if (activationCode is null)
                {
                    return AuthenticatorOperationResult.Failed(
                        "The authenticator activation code could not be generated.");
                }

                SteamClientResult<AuthenticatorFinalizationState> result = authenticationClient.FinalizeAuthenticator(
                    accessToken,
                    steamIdentifier,
                    activationCode,
                    enrolledAuthenticator.DeviceIdentifier,
                    smsActivationCode);

                if (!result.IsSuccessful || result.Value is AuthenticatorFinalizationState.Unknown)
                {
                    return AuthenticatorOperationResult.Failed(result.FailureReason);
                }

                if (result.Value is AuthenticatorFinalizationState.Succeeded)
                {
                    logger.Info(
                        SteamGuardOperation.AddAuthenticator,
                        OperationStatus.Success,
                        new LogInfo(SteamGuardInfoKey.AccountIdentifier, steamIdentifier));

                    return AuthenticatorOperationResult.Successful();
                }

                Thread.Sleep(FinalizationRetryDelay);
            }

            return AuthenticatorOperationResult.Failed("Steam did not activate the authenticator in time.");
        }

        /// <inheritdoc />
        public AuthenticatorOperationResult StartLostDeviceRecovery(string steamIdentifier, string accessToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(steamIdentifier, nameof(steamIdentifier));
            ArgumentException.ThrowIfNullOrWhiteSpace(accessToken, nameof(accessToken));

            SteamClientResult<bool> result = authenticationClient.StartAuthenticatorRevocation(
                steamIdentifier,
                accessToken);

            if (!result.IsSuccessful)
            {
                return AuthenticatorOperationResult.Failed(result.FailureReason);
            }

            logger.Info(
                SteamGuardOperation.RecoverAuthenticator,
                OperationStatus.Started,
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

            return AuthenticatorEnrolmentResult.Successful(result.Value);
        }

        /// <inheritdoc />
        public AuthenticatorOperationResult RemoveAllGuardCookies(string accessToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accessToken, nameof(accessToken));

            SteamClientResult<bool> result = authenticationClient.RemoveAllGuardCookies(accessToken);

            if (!result.IsSuccessful)
            {
                return AuthenticatorOperationResult.Failed(result.FailureReason);
            }

            logger.Info(SteamGuardOperation.RemoveAllGuardCookies, OperationStatus.Success);

            return AuthenticatorOperationResult.Successful();
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

        private string? GenerateActivationCode(EnrolledAuthenticator enrolledAuthenticator)
            => otpCodeGenerator.GenerateCode(enrolledAuthenticator.SharedSecret)?.Value;
    }
}
