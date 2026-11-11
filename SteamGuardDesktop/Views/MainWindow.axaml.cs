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
        public MainWindow() => InitializeComponent();

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
    }
}
