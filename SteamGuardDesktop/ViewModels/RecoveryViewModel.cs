using System;

using CommunityToolkit.Mvvm.ComponentModel;

using SteamGuardDesktop.Domain.Login;
using SteamGuardDesktop.Service.Account;
using SteamGuardDesktop.Service.Authenticator;
using SteamGuardDesktop.Service.Login;

namespace SteamGuardDesktop.ViewModels
{
    /// <summary>
    /// Presents the sign-in and authenticator recovery flow.
    /// </summary>
    public sealed partial class RecoveryViewModel : ViewModelBase
    {
        private readonly ILoginService loginService;

        private readonly IAuthenticatorManagementService authenticatorManagementService;

        private readonly IAccountService accountService;

        private LoginChallenge? challenge;

        private EnrolledAuthenticator? enrolledAuthenticator;

        [ObservableProperty]
        private string accountName = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string guardCode = string.Empty;

        [ObservableProperty]
        private string smsCode = string.Empty;

        [ObservableProperty]
        private string recoveryCode = string.Empty;

        [ObservableProperty]
        private string accessToken = string.Empty;

        [ObservableProperty]
        private string guardMessage = string.Empty;

        [ObservableProperty]
        private bool isSignInStarted;

        [ObservableProperty]
        private bool isGuardSubmitted;

        [ObservableProperty]
        private bool isGuardCodeRequired;

        [ObservableProperty]
        private bool isRecoveringLostDevice;

        [ObservableProperty]
        private bool isRecoveryStarted;

        [ObservableProperty]
        private string steamIdentifier = string.Empty;

        /// <summary>
        /// Initialises a new instance of the <see cref="RecoveryViewModel"/> class.
        /// </summary>
        /// <param name="loginService">The service that coordinates the sign-in exchange.</param>
        /// <param name="authenticatorManagementService">The service that manages the authenticator.</param>
        /// <param name="accountService">The service that stores account metadata.</param>
        public RecoveryViewModel(
            ILoginService loginService,
            IAuthenticatorManagementService authenticatorManagementService,
            IAccountService accountService)
        {
            ArgumentNullException.ThrowIfNull(loginService, nameof(loginService));
            ArgumentNullException.ThrowIfNull(authenticatorManagementService, nameof(authenticatorManagementService));
            ArgumentNullException.ThrowIfNull(accountService, nameof(accountService));

            this.loginService = loginService;
            this.authenticatorManagementService = authenticatorManagementService;
            this.accountService = accountService;
        }

        /// <summary>
        /// Gets a value indicating whether a code must be supplied to finish the sign-in.
        /// </summary>
        public bool IsCodeRequired => IsGuardCodeRequired;

        /// <summary>
        /// Starts the sign-in with the credentials entered in the form.
        /// </summary>
        public void BeginSignIn()
        {
            LoginStartResult result = loginService.BeginLogin(new LoginCredentials
            {
                AccountName = AccountName.Trim(),
                Password = Password
            });

            Password = string.Empty;

            if (!result.IsSuccessful || result.Challenge is null)
            {
                ReportStatus(result.FailureReason);

                return;
            }

            challenge = result.Challenge;
            SteamIdentifier = result.Challenge.SteamIdentifier;
            GuardMessage = result.Challenge.GuardMessage;

            IsGuardCodeRequired = ContainsCodeGuard(result.Challenge);
            IsSignInStarted = true;
            IsGuardSubmitted = false;

            ClearStatus();
        }

        /// <summary>
        /// Submits the guard code entered in the form.
        /// </summary>
        public void SubmitGuardCode()
        {
            if (challenge is null)
            {
                ReportStatus("Start the sign-in before submitting a code.");

                return;
            }

            LoginGuardResult result = loginService.SubmitGuardCode(challenge, GuardCode.Trim(), LoginGuardType.EmailCode);

            GuardCode = string.Empty;

            if (!result.IsSuccessful)
            {
                ReportStatus(result.FailureReason);

                return;
            }

            IsGuardSubmitted = true;
            ClearStatus();
        }

        /// <summary>
        /// Polls the sign-in until Steam issues its tokens.
        /// </summary>
        public void CompleteSignIn()
        {
            if (challenge is null)
            {
                ReportStatus("Start the sign-in before completing it.");

                return;
            }

            LoginCompletionResult result = loginService.CompleteLogin(challenge);

            if (!result.IsSuccessful || result.Tokens is null)
            {
                ReportStatus(result.FailureReason);

                return;
            }

            AccessToken = result.Tokens.AccessToken;

            if (!string.IsNullOrWhiteSpace(result.Tokens.SteamIdentifier))
            {
                SteamIdentifier = result.Tokens.SteamIdentifier;
            }

            IsSignInStarted = false;
            ClearStatus();
        }

        /// <summary>
        /// Starts the recovery of an authenticator whose device is no longer available.
        /// </summary>
        public void StartLostDeviceRecovery()
        {
            if (string.IsNullOrWhiteSpace(AccessToken) || string.IsNullOrWhiteSpace(SteamIdentifier))
            {
                ReportStatus("Sign in before starting the recovery.");

                return;
            }

            AuthenticatorStatusResult statusResult = authenticatorManagementService.GetStatus(SteamIdentifier, AccessToken);

            if (!statusResult.IsSuccessful)
            {
                ReportStatus(statusResult.FailureReason);

                return;
            }

            IsRecoveringLostDevice = statusResult.Status?.HasMobileAuthenticator == true;
            IsRecoveryStarted = true;
            ClearStatus();
        }

        /// <summary>
        /// Removes the existing authenticator with the revocation code entered in the form.
        /// </summary>
        public void RemoveAuthenticatorWithRevocationCode()
        {
            if (string.IsNullOrWhiteSpace(AccessToken))
            {
                ReportStatus("Sign in before removing the authenticator.");

                return;
            }

            AuthenticatorOperationResult result = authenticatorManagementService.RemoveAuthenticator(
                SteamIdentifier,
                AccessToken,
                RecoveryCode.Trim());

            RecoveryCode = string.Empty;

            if (!result.IsSuccessful)
            {
                ReportStatus(result.FailureReason);

                return;
            }

            IsRecoveringLostDevice = false;
            ReportStatus("The authenticator was removed.");
        }

        /// <summary>
        /// Enrols a new authenticator for the signed-in account.
        /// </summary>
        public void EnrolAuthenticator()
        {
            if (string.IsNullOrWhiteSpace(AccessToken) || string.IsNullOrWhiteSpace(SteamIdentifier))
            {
                ReportStatus("Sign in before enrolling an authenticator.");

                return;
            }

            AuthenticatorEnrolmentResult result = authenticatorManagementService.AddAuthenticator(
                SteamIdentifier,
                AccessToken);

            if (!result.IsSuccessful || result.Authenticator is null)
            {
                ReportStatus(result.FailureReason);

                return;
            }

            enrolledAuthenticator = result.Authenticator;
            GuardMessage = result.ActivationCode;
            ReportStatus("Activate the authenticator with the displayed code and the SMS code.");
        }

        /// <summary>
        /// Completes the enrolment with the activation code and the SMS code.
        /// </summary>
        public void FinalizeAuthenticator()
        {
            if (enrolledAuthenticator is null)
            {
                ReportStatus("Enrol an authenticator before activating it.");

                return;
            }

            AuthenticatorOperationResult result = authenticatorManagementService.FinalizeAuthenticator(
                SteamIdentifier,
                AccessToken,
                enrolledAuthenticator.DeviceIdentifier,
                string.Empty);

            if (!result.IsSuccessful)
            {
                ReportStatus(result.FailureReason);

                return;
            }

            AccountSaveResult saveResult = accountService.SaveAccount(
                new Domain.Account.SteamAccount
                {
                    SteamIdentifier = SteamIdentifier,
                    Label = AccountName.Trim()
                },
                authenticatorManagementService.CreateSecrets(enrolledAuthenticator));

            if (!saveResult.IsSuccessful)
            {
                ReportStatus(saveResult.FailureReason);

                return;
            }

            enrolledAuthenticator = null;
            ReportStatus("The authenticator was enrolled and the account was saved.");
        }

        /// <summary>
        /// Recovers an account whose authenticator device is no longer available.
        /// </summary>
        public void RecoverLostDevice()
        {
            if (string.IsNullOrWhiteSpace(AccessToken) || string.IsNullOrWhiteSpace(SteamIdentifier))
            {
                ReportStatus("Sign in before recovering the authenticator.");

                return;
            }

            AuthenticatorEnrolmentResult result = authenticatorManagementService.RecoverAuthenticator(
                SteamIdentifier,
                AccessToken,
                SmsCode.Trim());

            SmsCode = string.Empty;

            if (!result.IsSuccessful || result.Authenticator is null)
            {
                ReportStatus(result.FailureReason);

                return;
            }

            AccountSaveResult saveResult = accountService.SaveAccount(
                new Domain.Account.SteamAccount
                {
                    SteamIdentifier = SteamIdentifier,
                    Label = AccountName.Trim()
                },
                authenticatorManagementService.CreateSecrets(result.Authenticator));

            if (!saveResult.IsSuccessful)
            {
                ReportStatus(saveResult.FailureReason);

                return;
            }

            ReportStatus("The authenticator was recovered and the account was saved.");
        }

        /// <summary>
        /// Clears the form so that credentials are not retained.
        /// </summary>
        public void Reset()
        {
            AccountName = string.Empty;
            Password = string.Empty;
            GuardCode = string.Empty;
            SmsCode = string.Empty;
            RecoveryCode = string.Empty;
            AccessToken = string.Empty;
            GuardMessage = string.Empty;
            SteamIdentifier = string.Empty;
            challenge = null;
            enrolledAuthenticator = null;
            IsSignInStarted = false;
            IsGuardSubmitted = false;
            IsGuardCodeRequired = false;
            IsRecoveringLostDevice = false;
            IsRecoveryStarted = false;
        }

        /// <summary>
        /// Determines whether a challenge expects a code rather than an in-app approval.
        /// </summary>
        /// <param name="loginChallenge">The challenge reported by Steam.</param>
        /// <returns><c>true</c> when a code must be supplied.</returns>
        private static bool ContainsCodeGuard(LoginChallenge loginChallenge)
        {
            foreach (LoginGuardType guardType in loginChallenge.AcceptedGuards)
            {
                if (guardType == LoginGuardType.EmailCode)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
