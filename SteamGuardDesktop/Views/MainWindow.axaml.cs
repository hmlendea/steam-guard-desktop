using System;

using Avalonia.Controls;
using Avalonia.Interactivity;

using SteamGuardDesktop.Configuration;
using SteamGuardDesktop.ViewModels;

namespace SteamGuardDesktop.Views
{
    /// <summary>
    /// Represents the principal application window.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private static ThemePreference[] ThemePreferences =>
        [
            ThemePreference.System,
            ThemePreference.Light,
            ThemePreference.Dark
        ];

        /// <summary>
        /// Initialises a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            ThemeSelector.ItemsSource = ThemePreferences;
            ThemeSelector.SelectedIndex = 0;
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

        private void OnThemePreferenceChanged(object? sender, SelectionChangedEventArgs arguments)
        {
            if (ThemeSelector.SelectedItem is not ThemePreference selectedPreference)
            {
                return;
            }

            ViewModel?.ApplyThemePreference(selectedPreference);
        }

        private void OnWindowDeactivated(object? sender, EventArgs arguments) => ViewModel?.HandleWindowDeactivated();

        private void OnWindowClosed(object? sender, EventArgs arguments) => ViewModel?.ShutDown();
    }
}
