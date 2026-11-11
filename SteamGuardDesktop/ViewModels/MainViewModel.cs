using System;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Domain.Confirmations;
using SteamGuardDesktop.Service.Account;
using SteamGuardDesktop.Service.Confirmations;
using SteamGuardDesktop.Service.Platform;
using SteamGuardDesktop.Service.Storage;
using SteamGuardDesktop.Service.Theming;

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

        [ObservableProperty]
        private AccountItemViewModel? selectedAccount;

        [ObservableProperty]
        private ConfirmationItemViewModel? selectedConfirmation;

        [ObservableProperty]
        private SecretStoreAvailability secretStoreAvailability;

        [ObservableProperty]
        private PlatformTheme currentTheme;

        [ObservableProperty]
        private bool isAccountHidden;

        [ObservableProperty]
        private bool isDecisionInProgress;

        /// <summary>
        /// Initialises a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="accountService">The service that coordinates the accounts.</param>
        /// <param name="confirmationService">The service that coordinates the confirmations.</param>
        /// <param name="authenticatorViewModel">The authenticator view model.</param>
        /// <param name="accountSetupViewModel">The account setup view model.</param>
        /// <param name="clipboardWriter">The clipboard writer of the host platform.</param>
        /// <param name="themeService">The platform theme service.</param>
        public MainViewModel(
            IAccountService accountService,
            IConfirmationService confirmationService,
            AuthenticatorViewModel authenticatorViewModel,
            AccountSetupViewModel accountSetupViewModel,
            IClipboardWriter clipboardWriter,
            IThemeService themeService)
        {
            ArgumentNullException.ThrowIfNull(accountService, nameof(accountService));
            ArgumentNullException.ThrowIfNull(confirmationService, nameof(confirmationService));
            ArgumentNullException.ThrowIfNull(authenticatorViewModel, nameof(authenticatorViewModel));
            ArgumentNullException.ThrowIfNull(accountSetupViewModel, nameof(accountSetupViewModel));
            ArgumentNullException.ThrowIfNull(clipboardWriter, nameof(clipboardWriter));
            ArgumentNullException.ThrowIfNull(themeService, nameof(themeService));

            this.accountService = accountService;
            this.confirmationService = confirmationService;
            this.clipboardWriter = clipboardWriter;
            this.themeService = themeService;

            Authenticator = authenticatorViewModel;
            AccountSetup = accountSetupViewModel;
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
        /// Loads the configured accounts and their current state.
        /// </summary>
        public void Load()
        {
            SecretStoreAvailability = accountService.GetSecretStoreAvailability();
            CurrentTheme = themeService.GetCurrentTheme();

            Accounts.Clear();

            foreach (SteamAccount account in accountService.GetAccounts())
            {
                Accounts.Add(new AccountItemViewModel(account));
            }

            OnPropertyChanged(nameof(HasAccounts));
            OnPropertyChanged(nameof(IsSecretStoreUnavailable));

            SelectedAccount = Accounts.FirstOrDefault();
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
                ReportStatus("Select an account first.");

                return;
            }

            AuthenticatorSecrets? secrets = accountService.GetSecrets(SelectedAccount.Account);

            if (secrets is null)
            {
                Authenticator.Clear();
                ReportStatus("The account secrets are unavailable.");

                return;
            }

            ClearStatus();
            Authenticator.Refresh(secrets.SharedSecret);
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
        public void ToggleAccountVisibility() => IsAccountHidden = !IsAccountHidden;

        private void ReplaceConfirmations(ConfirmationListResult result)
        {
            Confirmation[] confirmations = [.. result.Confirmations];

            Confirmations.Clear();

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

        private static string DescribeFailure(string failureReason, bool requiresAuthentication)
        {
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
    }
}
