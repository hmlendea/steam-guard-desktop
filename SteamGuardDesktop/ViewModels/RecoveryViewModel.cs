using System;
using System.Collections.Generic;

using CommunityToolkit.Mvvm.ComponentModel;

using SteamGuardDesktop.Domain.Account;
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
        private string issuedRevocationCode = string.Empty;

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
        private bool hasPendingActivation;

        [ObservableProperty]
        private int signOutRequestCount;

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
        /// Gets a value indicating whether an account has been saved since the last change.
        /// </summary>
        public event EventHandler? AccountSaved;

        /// <summary>
        /// Gets a value indicating whether an SMS code must be supplied to finish the current step.
        /// </summary>
        public bool IsSmsCodeRequired => IsRecoveringLostDevice || HasPendingActivation;

        /// <summary>
        /// Gets a value indicating whether an issued revocation code must be shown to the user.
        /// </summary>
        public bool HasIssuedRevocationCode => !string.IsNullOrWhiteSpace(IssuedRevocationCode);

        /// <summary>
        /// Gets a value indicating whether a new authenticator may be enrolled, which is not the case while an
        /// issued authenticator is still waiting to be activated.
        /// </summary>
        public bool CanEnrolAuthenticator => !HasPendingActivation;

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

            LoginGuardType guardType = ComposeGuardType(challenge);

            if (guardType == LoginGuardType.Unsupported)
            {
                ReportStatus("Steam did not accept a code for this sign-in. Approve the sign-in on the device instead.");

                return;
            }

            LoginGuardResult result = loginService.SubmitGuardCode(challenge, GuardCode.Trim(), guardType);

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
            IsGuardSubmitted = true;

            if (!accountService.IsAccountConfigured(SteamIdentifier))
            {
                ReportStatus("The sign-in succeeded. Enrol or recover an authenticator to save the account and store the access token.");

                return;
            }

            AccountAccessTokenResult tokenResult = accountService.SaveAccessToken(SteamIdentifier, AccessToken);

            if (!tokenResult.IsSuccessful)
            {
                ReportStatus("The sign-in succeeded, but the access token could not be stored: " + tokenResult.FailureReason);

                return;
            }

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

            if (!IsRecoveringLostDevice)
            {
                ReportStatus("No mobile authenticator is attached to this account.");

                return;
            }

            AuthenticatorOperationResult startResult = authenticatorManagementService.StartLostDeviceRecovery(
                SteamIdentifier,
                AccessToken);

            if (!startResult.IsSuccessful)
            {
                IsRecoveringLostDevice = false;
                ReportStatus(startResult.FailureReason);

                return;
            }

            ReportStatus("Steam sent a code to the registered phone number. Enter it to recover the authenticator.");
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

            if (HasPendingActivation)
            {
                ReportStatus(
                    "An authenticator is already waiting to be activated. Activate it, or restart the enrolment " +
                    "after storing the revocation code that was shown.");

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
            HasPendingActivation = true;
            IssuedRevocationCode = result.Authenticator.RevocationCode;
            ReportStatus(
                "Steam sent an SMS code to the registered phone number. Store the revocation code shown above, then " +
                "enter the SMS code and activate the authenticator.");
        }

        /// <summary>
        /// Completes the enrolment with the activation code derived from the issued shared secret and the SMS code.
        /// </summary>
        public void FinalizeAuthenticator()
        {
            if (enrolledAuthenticator is null)
            {
                ReportStatus("Enrol an authenticator before activating it.");

                return;
            }

            if (string.IsNullOrWhiteSpace(SmsCode))
            {
                ReportStatus("Enter the SMS code that Steam sent to the registered phone number.");

                return;
            }

            AuthenticatorOperationResult result = authenticatorManagementService.FinalizeAuthenticator(
                SteamIdentifier,
                AccessToken,
                enrolledAuthenticator,
                SmsCode.Trim());

            SmsCode = string.Empty;

            if (!result.IsSuccessful)
            {
                ReportStatus(result.FailureReason);

                return;
            }

            AccountSaveResult saveResult = accountService.SaveAccount(
                new SteamAccount
                {
                    SteamIdentifier = SteamIdentifier,
                    Label = string.IsNullOrWhiteSpace(AccountName) ? SteamIdentifier : AccountName.Trim()
                },
                authenticatorManagementService.CreateSecrets(enrolledAuthenticator));

            if (!saveResult.IsSuccessful)
            {
                ReportStatus(ComposeAccountFailure(saveResult));

                return;
            }

            AccountAccessTokenResult tokenResult = SaveRecoveredAccessToken();

            if (!tokenResult.IsSuccessful)
            {
                ReportStatus(tokenResult.FailureReason);

                return;
            }

            enrolledAuthenticator = null;
            HasPendingActivation = false;
            AccountSaved?.Invoke(this, EventArgs.Empty);
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
                new SteamAccount
                {
                    SteamIdentifier = SteamIdentifier,
                    Label = string.IsNullOrWhiteSpace(AccountName) ? SteamIdentifier : AccountName.Trim()
                },
                authenticatorManagementService.CreateSecrets(result.Authenticator));

            if (!saveResult.IsSuccessful)
            {
                ReportStatus(ComposeAccountFailure(saveResult));

                return;
            }

            AccountAccessTokenResult tokenResult = SaveRecoveredAccessToken();

            if (!tokenResult.IsSuccessful)
            {
                ReportStatus(tokenResult.FailureReason);

                return;
            }

            AccountSaved?.Invoke(this, EventArgs.Empty);
            ReportStatus("The authenticator was recovered and the account was saved.");
        }

        /// <summary>
        /// Signs out of the account and discards the session material held by this form.
        /// </summary>
        public void SignOut()
        {
            if (!string.IsNullOrWhiteSpace(AccessToken))
            {
                authenticatorManagementService.RemoveAllGuardCookies(AccessToken);
            }

            challenge = null;
            enrolledAuthenticator = null;
            AccessToken = string.Empty;
            GuardCode = string.Empty;
            SmsCode = string.Empty;
            RecoveryCode = string.Empty;
            IssuedRevocationCode = string.Empty;
            GuardMessage = string.Empty;
            IsSignInStarted = false;
            IsGuardSubmitted = false;
            IsGuardCodeRequired = false;
            IsRecoveringLostDevice = false;
            HasPendingActivation = false;
            SignOutRequestCount += 1;
            ClearStatus();
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
            IssuedRevocationCode = string.Empty;
            AccessToken = string.Empty;
            GuardMessage = string.Empty;
            SteamIdentifier = string.Empty;
            challenge = null;
            enrolledAuthenticator = null;
            IsSignInStarted = false;
            IsGuardSubmitted = false;
            IsGuardCodeRequired = false;
            IsRecoveringLostDevice = false;
            HasPendingActivation = false;
        }

        private AccountAccessTokenResult SaveRecoveredAccessToken()
        {
            if (string.IsNullOrWhiteSpace(AccessToken))
            {
                return AccountAccessTokenResult.Successful();
            }

            return accountService.SaveAccessToken(SteamIdentifier, AccessToken);
        }

        private static string ComposeAccountFailure(AccountSaveResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.FailureReason))
            {
                return result.FailureReason;
            }

            return string.Join(" ", result.ValidationErrors);
        }

        /// <summary>
        /// Selects the guard type that a challenge expects.
        /// </summary>
        /// <param name="loginChallenge">The challenge reported by Steam.</param>
        /// <returns>The guard type to submit, or <see cref="LoginGuardType.Unsupported"/>.</returns>
        private static LoginGuardType ComposeGuardType(LoginChallenge loginChallenge)
        {
            foreach (LoginGuardType guardType in loginChallenge.AcceptedGuards)
            {
                if (guardType is LoginGuardType.EmailCode or LoginGuardType.DeviceCode)
                {
                    return guardType;
                }
            }

            return LoginGuardType.Unsupported;
        }

        private static bool ContainsCodeGuard(LoginChallenge loginChallenge)
            => ComposeGuardType(loginChallenge) != LoginGuardType.Unsupported;

        /// <inheritdoc />
        partial void OnIsGuardCodeRequiredChanged(bool value) => OnPropertyChanged(nameof(IsCodeRequired));

        /// <inheritdoc />
        partial void OnIsRecoveringLostDeviceChanged(bool value) => OnPropertyChanged(nameof(IsSmsCodeRequired));

        /// <inheritdoc />
        partial void OnHasPendingActivationChanged(bool value)
        {
            OnPropertyChanged(nameof(IsSmsCodeRequired));
            OnPropertyChanged(nameof(CanEnrolAuthenticator));
        }

        /// <inheritdoc />
        partial void OnIssuedRevocationCodeChanged(string value) => OnPropertyChanged(nameof(HasIssuedRevocationCode));
    }
}
