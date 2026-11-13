using System;

using Avalonia.Controls;
using Avalonia.Interactivity;

using SteamGuardDesktop.ViewModels;

namespace SteamGuardDesktop.Views
{
    /// <summary>
    /// Represents the principal application window.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Deactivated += OnWindowDeactivated;
            Closed += OnWindowClosed;
        }

        private MainViewModel? ViewModel => DataContext as MainViewModel;

        private void OnRefreshCodeClick(object? sender, RoutedEventArgs arguments) => ViewModel?.RefreshCode();

        private void OnCopyCodeClick(object? sender, RoutedEventArgs arguments) => ViewModel?.CopyCode();

        private void OnRefreshConfirmationsClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.RefreshConfirmations();

        private void OnRemoveAccountClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.RemoveSelectedAccount();

        private void OnApproveClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.ApproveSelectedConfirmation();

        private void OnDenyClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.DenySelectedConfirmation();

        private void OnSaveAccountClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.SaveAccount();

        private void OnShowDetailsClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.LoadSelectedConfirmationDetails();

        private void OnToggleAccountVisibilityClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.ToggleAccountVisibility();

        private void OnToggleSecretVisibilityClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.AccountSetup.ToggleSecretVisibility();

        private void OnBeginSignInClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.Recovery.BeginSignIn();

        private void OnSubmitGuardCodeClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.Recovery.SubmitGuardCode();

        private void OnCompleteSignInClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.Recovery.CompleteSignIn();

        private void OnCheckAuthenticatorStatusClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.Recovery.RefreshAuthenticatorStatus();

        private void OnStartLostDeviceRecoveryClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.Recovery.StartLostDeviceRecovery();

        private void OnRemoveAuthenticatorClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.Recovery.RemoveAuthenticatorWithRevocationCode();

        private void OnEnrolAuthenticatorClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.Recovery.EnrolAuthenticator();

        private void OnFinalizeAuthenticatorClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.Recovery.FinalizeAuthenticator();

        private void OnRecoverLostDeviceClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.Recovery.RecoverLostDevice();

        private void OnSignOutClick(object? sender, RoutedEventArgs arguments)
            => ViewModel?.Recovery.SignOut();

        private void OnWindowDeactivated(object? sender, EventArgs arguments) => ViewModel?.HandleWindowDeactivated();

        private void OnWindowClosed(object? sender, EventArgs arguments) => ViewModel?.ShutDown();
    }
}
