using System;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using NuciLog.Core;

using SteamGuardDesktop.Configuration;
using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Domain.Confirmations;
using SteamGuardDesktop.Logging;
using SteamGuardDesktop.Service.Account;
using SteamGuardDesktop.Service.Configuration;
using SteamGuardDesktop.Service.Confirmations;
using SteamGuardDesktop.Service.Platform;
using SteamGuardDesktop.Service.Storage;
using SteamGuardDesktop.Service.Theming;
using SteamGuardDesktop.Service.Time;

namespace SteamGuardDesktop.ViewModels
{
    /// <summary>
    /// Coordinates the account, authenticator, and confirmation state presented by the main window.
    /// </summary>
    public sealed partial class MainViewModel : ViewModelBase
    {
        private readonly IAccountService accountService;

        private readonly IConfirmationService confirmationService;

        private readonly IClipboardWriter clipboardWriter;

        private readonly IThemeService themeService;

        private readonly IClockDriftService clockDriftService;

        private readonly ITicker ticker;

        private readonly SteamGuardSettings settings;

        private readonly ISettingsStore settingsStore;

        private readonly ILogger logger;

        [ObservableProperty]
        private AccountItemViewModel? selectedAccount;

        [ObservableProperty]
        private ConfirmationItemViewModel? selectedConfirmation;

        [ObservableProperty]
        private SecretStoreAvailability secretStoreAvailability;

        [ObservableProperty]
        private bool isAccountHidden;

        [ObservableProperty]
        private bool isDecisionInProgress;

        [ObservableProperty]
        private string confirmationDetails = string.Empty;

        /// <summary>
        /// Initialises a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="accountService">The service that coordinates the accounts.</param>
        /// <param name="confirmationService">The service that coordinates the confirmations.</param>
        /// <param name="authenticatorViewModel">The authenticator view model.</param>
        /// <param name="accountSetupViewModel">The account setup view model.</param>
        /// <param name="recoveryViewModel">The sign-in and authenticator recovery view model.</param>
        /// <param name="clipboardWriter">The clipboard writer of the host platform.</param>
        /// <param name="themeService">The platform theme service.</param>
        /// <param name="clockDriftService">The service that compares the local clock with the Steam clock.</param>
        /// <param name="ticker">The ticker that drives the countdown.</param>
        /// <param name="settings">The application settings.</param>
        /// <param name="settingsStore">The store that persists the user settings.</param>
        /// <param name="logger">The diagnostics logger.</param>
        public MainViewModel(
            IAccountService accountService,
            IConfirmationService confirmationService,
            AuthenticatorViewModel authenticatorViewModel,
            AccountSetupViewModel accountSetupViewModel,
            RecoveryViewModel recoveryViewModel,
            IClipboardWriter clipboardWriter,
            IThemeService themeService,
            IClockDriftService clockDriftService,
            ITicker ticker,
            SteamGuardSettings settings,
            ISettingsStore settingsStore,
            ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(accountService, nameof(accountService));
            ArgumentNullException.ThrowIfNull(confirmationService, nameof(confirmationService));
            ArgumentNullException.ThrowIfNull(authenticatorViewModel, nameof(authenticatorViewModel));
            ArgumentNullException.ThrowIfNull(accountSetupViewModel, nameof(accountSetupViewModel));
            ArgumentNullException.ThrowIfNull(recoveryViewModel, nameof(recoveryViewModel));
            ArgumentNullException.ThrowIfNull(clipboardWriter, nameof(clipboardWriter));
            ArgumentNullException.ThrowIfNull(themeService, nameof(themeService));
            ArgumentNullException.ThrowIfNull(clockDriftService, nameof(clockDriftService));
            ArgumentNullException.ThrowIfNull(ticker, nameof(ticker));
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ArgumentNullException.ThrowIfNull(settingsStore, nameof(settingsStore));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            this.accountService = accountService;
            this.confirmationService = confirmationService;
            this.clipboardWriter = clipboardWriter;
            this.themeService = themeService;
            this.clockDriftService = clockDriftService;
            this.ticker = ticker;
            this.settings = settings;
            this.settingsStore = settingsStore;
            this.logger = logger;

            Authenticator = authenticatorViewModel;
            AccountSetup = accountSetupViewModel;
            Recovery = recoveryViewModel;

            recoveryViewModel.AccountSaved += OnRecoveryAccountSaved;
            recoveryViewModel.PropertyChanged += OnRecoveryPropertyChanged;

            ApplyPersistedSettings();
            themeService.ApplyTheme();
            ticker.Tick += OnTickerTick;
        }

        /// <summary>
        /// Gets or sets a value indicating whether account details are hidden while the window is inactive.
        /// </summary>
        public bool IsAccountLockingEnabled
        {
            get => settings.IsAccountHiddenWhenInactive;

            set
            {
                if (settings.IsAccountHiddenWhenInactive == value)
                {
                    return;
                }

                settings.IsAccountHiddenWhenInactive = value;
                OnPropertyChanged();
                PersistSettings();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether secrets may be revealed on explicit request.
        /// </summary>
        public bool IsSecretRevealEnabled
        {
            get => settings.IsSecretRevealAllowed;

            set
            {
                if (settings.IsSecretRevealAllowed == value)
                {
                    return;
                }

                settings.IsSecretRevealAllowed = value;
                OnPropertyChanged();
                AccountSetup.NotifySecretRevealAvailabilityChanged();
                PersistSettings();
            }
        }

        /// <summary>
        /// Gets the configured accounts.
        /// </summary>
        public ObservableCollection<AccountItemViewModel> Accounts { get; } = [];

        /// <summary>
        /// Gets the pending confirmations of the selected account.
        /// </summary>
        public ObservableCollection<ConfirmationItemViewModel> Confirmations { get; } = [];

        /// <summary>
        /// Gets the authenticator view model.
        /// </summary>
        public AuthenticatorViewModel Authenticator { get; }

        /// <summary>
        /// Gets the account setup view model.
        /// </summary>
        public AccountSetupViewModel AccountSetup { get; }

        /// <summary>
        /// Gets the sign-in and authenticator recovery view model.
        /// </summary>
        public RecoveryViewModel Recovery { get; }

        /// <summary>
        /// Gets a value indicating whether at least one account is configured.
        /// </summary>
        public bool HasAccounts => Accounts.Count > 0;

        /// <summary>
        /// Gets a value indicating whether pending confirmations are available.
        /// </summary>
        public bool HasConfirmations => Confirmations.Count > 0;

        /// <summary>
        /// Gets a value indicating whether the credential store is available.
        /// </summary>
        public bool IsSecretStoreUnavailable => SecretStoreAvailability != SecretStoreAvailability.Available;

        /// <summary>
        /// Gets a value indicating whether a failure must be reported to the user.
        /// </summary>
        public bool HasFailure => !string.IsNullOrWhiteSpace(StatusMessage);

        /// <summary>
        /// Gets a value indicating whether account details are currently concealed.
        /// </summary>
        public bool AreAccountDetailsConcealed => IsAccountHidden;

        /// <summary>
        /// Gets a value indicating whether the details of a confirmation are available.
        /// </summary>
        public bool HasConfirmationDetails => !string.IsNullOrWhiteSpace(ConfirmationDetails);

        /// <summary>
        /// Loads the configured accounts and their current state.
        /// </summary>
        public void Load()
        {
            SecretStoreAvailability = accountService.GetSecretStoreAvailability();
            AppTheme appliedTheme = themeService.ApplyTheme();

            logger.Info(
                SteamGuardOperation.ApplyTheme,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.AppTheme, appliedTheme.ToString()));

            IsAccountHidden = false;

            Accounts.Clear();

            foreach (SteamAccount account in accountService.GetAccounts())
            {
                Accounts.Add(new AccountItemViewModel(account));
            }

            logger.Info(
                SteamGuardOperation.LaunchApplication,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.AccountCount, Accounts.Count),
                new LogInfo(SteamGuardInfoKey.SecretStoreAvailability, SecretStoreAvailability.ToString()));

            OnPropertyChanged(nameof(HasAccounts));
            OnPropertyChanged(nameof(IsSecretStoreUnavailable));
            OnPropertyChanged(nameof(AreAccountDetailsConcealed));

            SelectedAccount = Accounts.FirstOrDefault();

            RefreshCode();
            EvaluateClockDrift();

            ticker.Start();
        }

        /// <summary>
        /// Adds or updates the account described by the setup form.
        /// </summary>
        public void SaveAccount()
        {
            AccountSaveResult result = accountService.SaveAccount(
                AccountSetup.BuildAccount(),
                AccountSetup.BuildSecrets());

            if (!result.IsSuccessful)
            {
                ReportStatus(ComposeFailureMessage(result));

                return;
            }

            AccountSetup.Reset();
            ClearStatus();
            Load();
        }

        /// <summary>
        /// Removes the selected account and its stored secrets.
        /// </summary>
        public void RemoveSelectedAccount()
        {
            if (SelectedAccount is null)
            {
                ReportStatus("Select an account first.");

                return;
            }

            AccountRemovalResult result = accountService.RemoveAccount(SelectedAccount.Identifier);

            if (!result.IsSuccessful)
            {
                ReportStatus(result.FailureReason);

                return;
            }

            Confirmations.Clear();
            ConfirmationDetails = string.Empty;
            Authenticator.Clear();
            ClearStatus();
            Load();
        }

        /// <summary>
        /// Refreshes the displayed authentication code.
        /// </summary>
        public void RefreshCode()
        {
            if (SelectedAccount is null)
            {
                Authenticator.Clear();

                return;
            }

            AuthenticatorSecrets? secrets = accountService.GetSecrets(SelectedAccount.Account);

            if (secrets is null)
            {
                Authenticator.Clear();
                ReportStatus("The account secrets are unavailable.");

                return;
            }

            Authenticator.Refresh(secrets.SharedSecret);

            logger.Debug(
                SteamGuardOperation.GenerateCode,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.AccountIdentifier, SelectedAccount.Identifier));
        }

        /// <summary>
        /// Copies the displayed authentication code to the clipboard.
        /// </summary>
        public void CopyCode()
        {
            if (!Authenticator.HasCode)
            {
                ReportStatus("There is no code to copy.");

                return;
            }

            if (!clipboardWriter.Copy(Authenticator.CurrentCode))
            {
                ReportStatus("The code could not be copied to the clipboard.");

                return;
            }

            ReportStatus("The code was copied to the clipboard.");
        }

        /// <summary>
        /// Refreshes the pending confirmations of the selected account.
        /// </summary>
        public void RefreshConfirmations()
        {
            if (SelectedAccount is null)
            {
                ReportStatus("Select an account first.");

                return;
            }

            IsBusy = true;

            ConfirmationListResult result = confirmationService.GetConfirmations(SelectedAccount.Account);

            IsBusy = false;

            if (!result.IsSuccessful)
            {
                Confirmations.Clear();
                OnPropertyChanged(nameof(HasConfirmations));
                ReportStatus(DescribeFailure(result.FailureReason, result.RequiresAuthentication));

                return;
            }

            ReplaceConfirmations(result);
        }

        /// <summary>
        /// Loads the descriptive text of the selected confirmation.
        /// </summary>
        public void LoadSelectedConfirmationDetails()
        {
            if (SelectedAccount is null || SelectedConfirmation is null)
            {
                ReportStatus("Select a confirmation first.");

                return;
            }

            IsBusy = true;

            ConfirmationDetailsResult result = confirmationService.GetConfirmationDetails(
                SelectedAccount.Account,
                SelectedConfirmation.Confirmation.Identifier);

            IsBusy = false;

            if (!result.IsSuccessful)
            {
                ConfirmationDetails = string.Empty;
                ReportStatus(result.FailureReason);

                return;
            }

            ConfirmationDetails = result.Details;
            ClearStatus();
        }

        /// <summary>
        /// Approves the selected confirmation.
        /// </summary>
        public void ApproveSelectedConfirmation() => ApplyDecision(ConfirmationDecisionType.Approve);

        /// <summary>
        /// Denies the selected confirmation.
        /// </summary>
        public void DenySelectedConfirmation() => ApplyDecision(ConfirmationDecisionType.Deny);

        /// <summary>
        /// Toggles whether account details are hidden.
        /// </summary>
        public void ToggleAccountVisibility()
        {
            IsAccountHidden = !IsAccountHidden;
            OnPropertyChanged(nameof(AreAccountDetailsConcealed));

            if (IsAccountHidden)
            {
                ConfirmationDetails = string.Empty;
                Authenticator.Clear();
                Confirmations.Clear();
                OnPropertyChanged(nameof(HasConfirmations));
            }
            else
            {
                RefreshCode();
            }
        }

        /// <summary>
        /// Conceals account details when the window loses focus and the setting is enabled.
        /// </summary>
        public void HandleWindowDeactivated()
        {
            if (!settings.IsAccountHiddenWhenInactive || IsAccountHidden)
            {
                return;
            }

            IsAccountHidden = true;
            ConfirmationDetails = string.Empty;
            Authenticator.Clear();
            Confirmations.Clear();

            OnPropertyChanged(nameof(AreAccountDetailsConcealed));
            OnPropertyChanged(nameof(HasConfirmations));
        }

        /// <summary>
        /// Stops background signalling when the window is closing.
        /// </summary>
        public void ShutDown()
        {
            Recovery.AccountSaved -= OnRecoveryAccountSaved;
            Recovery.PropertyChanged -= OnRecoveryPropertyChanged;
            ticker.Tick -= OnTickerTick;
            ticker.Stop();
        }

        private void ApplyPersistedSettings()
        {
            SettingsSnapshot snapshot = settingsStore.Load();

            settings.IsAccountHiddenWhenInactive = snapshot.IsAccountHiddenWhenInactive;
            settings.IsSecretRevealAllowed = snapshot.IsSecretRevealAllowed;
        }

        private void PersistSettings()
        {
            bool wasSaved = settingsStore.Save(new SettingsSnapshot
            {
                IsAccountHiddenWhenInactive = settings.IsAccountHiddenWhenInactive,
                IsSecretRevealAllowed = settings.IsSecretRevealAllowed
            });

            if (!wasSaved)
            {
                logger.Warn(
                    SteamGuardOperation.StoreSecrets,
                    OperationStatus.Failure,
                    new LogInfo(SteamGuardInfoKey.FailureReason, "The settings file could not be written."));

                ReportStatus("The preference could not be saved to disk.");
            }
        }

        private void OnRecoveryAccountSaved(object? sender, EventArgs arguments) => Load();

        private void OnRecoveryPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs arguments)
        {
            if (arguments.PropertyName == nameof(RecoveryViewModel.SignOutRequestCount))
            {
                IsAccountHidden = false;
                OnPropertyChanged(nameof(AreAccountDetailsConcealed));
                Load();
            }
        }

        private void OnTickerTick(object? sender, EventArgs arguments)
        {
            Authenticator.UpdateCountdown();

            if (!Authenticator.HasExpired() || SelectedAccount is null)
            {
                return;
            }

            RefreshCode();
        }

        private void EvaluateClockDrift()
        {
            ClockDriftReport? driftReport = clockDriftService.Evaluate();

            if (driftReport is null)
            {
                return;
            }

            Authenticator.ApplyClockDrift(driftReport);
        }

        private void ReplaceConfirmations(ConfirmationListResult result)
        {
            Confirmation[] confirmations = [.. result.Confirmations];

            Confirmations.Clear();
            ConfirmationDetails = string.Empty;
            SelectedConfirmation = null;

            foreach (Confirmation confirmation in confirmations)
            {
                Confirmations.Add(new ConfirmationItemViewModel(confirmation));
            }

            OnPropertyChanged(nameof(HasConfirmations));

            if (confirmations.Length == 0)
            {
                ReportStatus("There are no pending confirmations.");

                return;
            }

            ClearStatus();
        }

        private void ApplyDecision(ConfirmationDecisionType decisionType)
        {
            if (IsDecisionInProgress)
            {
                return;
            }

            if (SelectedAccount is null || SelectedConfirmation is null)
            {
                ReportStatus("Select a confirmation first.");

                return;
            }

            if (!SelectedConfirmation.CanBeDecided)
            {
                ReportStatus("This confirmation type cannot be handled by the application.");

                return;
            }

            ConfirmationDecision decision = new()
            {
                ConfirmationIdentifier = SelectedConfirmation.Confirmation.Identifier,
                Nonce = SelectedConfirmation.Confirmation.Nonce,
                DecisionType = decisionType
            };

            IsDecisionInProgress = true;

            ConfirmationDecisionResult result = confirmationService.ApplyDecision(SelectedAccount.Account, decision);

            IsDecisionInProgress = false;

            if (!result.IsSuccessful)
            {
                ReportStatus(DescribeFailure(result.FailureReason, result.RequiresAuthentication));

                return;
            }

            RefreshConfirmations();

            ReportStatus(decisionType == ConfirmationDecisionType.Approve
                ? "The confirmation was approved."
                : "The confirmation was denied.");
        }

        private static string ComposeFailureMessage(AccountSaveResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.FailureReason))
            {
                return result.FailureReason;
            }

            return string.Join(" ", result.ValidationErrors);
        }

        private static string DescribeFailure(string failureReason, bool requiresAuthentication){
            if (requiresAuthentication)
            {
                return "The Steam session is no longer valid. A new access token is required.";
            }

            if (string.IsNullOrWhiteSpace(failureReason))
            {
                return "The request could not be completed.";
            }

            return failureReason;
        }

        /// <inheritdoc />
        partial void OnConfirmationDetailsChanged(string value) => OnPropertyChanged(nameof(HasConfirmationDetails));
    }
}
