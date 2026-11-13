using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

using SteamGuardDesktop.ViewModels;
using SteamGuardDesktop.Views;

namespace SteamGuardDesktop
{
    /// <summary>
    /// Represents the Steam Guard Desktop application.
    /// </summary>
    public sealed partial class App : Application
    {
        private ServiceProvider? serviceProvider;

        /// <inheritdoc />
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        /// <inheritdoc />
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                serviceProvider = ServiceConfiguration.BuildServiceProvider();

                MainViewModel mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
                mainViewModel.Load();

                desktopLifetime.MainWindow = new MainWindow
                {
                    DataContext = mainViewModel
                };

                desktopLifetime.Exit += OnDesktopLifetimeExit;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void OnDesktopLifetimeExit(object? sender, ControlledApplicationLifetimeExitEventArgs arguments)
        {
            serviceProvider?.Dispose();
            serviceProvider = null;
        }
    }
}
