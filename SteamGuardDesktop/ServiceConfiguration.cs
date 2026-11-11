using System;
using System.Net.Http;

using Microsoft.Extensions.DependencyInjection;

using NuciLog;
using NuciLog.Configuration;
using NuciLog.Core;
using NuciWeb.HTTP;

using SteamGuard.TOTP;
using SteamGuardDesktop.Client.Steam;
using SteamGuardDesktop.Configuration;
using SteamGuardDesktop.DataAccess.Accounts;
using SteamGuardDesktop.Logging;
using SteamGuardDesktop.Service.Account;
using SteamGuardDesktop.Service.Authenticator;
using SteamGuardDesktop.Service.Confirmations;
using SteamGuardDesktop.Service.Diagnostics;
using SteamGuardDesktop.Service.Platform;
using SteamGuardDesktop.Service.Storage;
using SteamGuardDesktop.Service.Theming;
using SteamGuardDesktop.Service.Time;
using SteamGuardDesktop.ViewModels;

using TOTPSteamGuard = SteamGuard.TOTP.SteamGuard;

namespace SteamGuardDesktop
{
    /// <summary>
    /// Builds the service graph used by the application.
    /// </summary>
    public static class ServiceConfiguration
    {
        private static string ApplicationDataDirectoryName => "SteamGuardDesktop";

        private static string AccountStoreFileName => "accounts.bin";

        private static string SecretStoreFileName => "secrets.bin";

        /// <summary>
        /// Creates the service provider used by the application.
        /// </summary>
        /// <returns>The configured service provider.</returns>
        public static ServiceProvider BuildServiceProvider()
        {
            ServiceCollection services = new();
            SteamGuardSettings settings = new();
            DiagnosticsSettings diagnosticsSettings = new();
            string dataDirectory = ComposeDataDirectoryPath();
            string keySeed = ComposeKeySeed(dataDirectory);

            services.AddSingleton(settings);
            services.AddSingleton(diagnosticsSettings);
            services.AddSingleton(CreateLogger(diagnosticsSettings));

            services.AddSingleton<IClock, SystemClock>();
            services.AddSingleton<ISecretRedactor, SecretRedactor>();
            services.AddSingleton<IPlatformThemeResolver, PlatformThemeResolver>();
            services.AddSingleton<IThemeService, ThemeService>();

            services.AddSingleton<ISteamGuard, TOTPSteamGuard>();
            services.AddSingleton<IOtpCodeGenerator, OtpCodeGenerator>();
            services.AddSingleton<IDeviceIdentityProvider, DeviceIdentityProvider>();
            services.AddSingleton<IConfirmationRequestSigner, ConfirmationRequestSigner>();
            services.AddSingleton<ConfirmationRequestBuilder>();
            services.AddSingleton<ISteamResponseParser, SteamResponseParser>();

            services.AddSingleton<INativeSecretBackend>(_ => NativeSecretBackendFactory.Create());
            services.AddSingleton<ISecretStore>(serviceProvider => new ResilientSecretStore(
                serviceProvider.GetRequiredService<INativeSecretBackend>(),
                new EncryptedFileSecretStore(
                    System.IO.Path.Combine(dataDirectory, SecretStoreFileName),
                    keySeed)));

            services.AddSingleton<IAccountStore>(_ => new EncryptedFileAccountStore(
                System.IO.Path.Combine(dataDirectory, AccountStoreFileName),
                keySeed));

            services.AddSingleton<IAccountService, AccountService>();
            services.AddSingleton<IConfirmationService, ConfirmationService>();
            services.AddSingleton<HttpClient>(_ => CreateHttpClient(settings));
            services.AddSingleton<ISteamConfirmationClient, SteamConfirmationClient>();

            services.AddSingleton<IClipboardWriter, AvaloniaClipboardWriter>();
            services.AddSingleton<AuthenticatorViewModel>();
            services.AddSingleton<AccountSetupViewModel>();
            services.AddSingleton<MainViewModel>();

            return services.BuildServiceProvider();
        }

        private static ILogger CreateLogger(DiagnosticsSettings settings)
        {
            NuciLoggerSettings loggerSettings = new()
            {
                MinimumLevel = settings.MinimumLevel,
                TimestampFormat = settings.TimestampFormat,
                LogLineFormat = settings.LogLineFormat,
                LogFilePath = settings.LogFilePath,
                IsFileOutputEnabled = settings.IsFileOutputEnabled
            };

            return new SteamGuardLogger(loggerSettings);
        }

        private static HttpClient CreateHttpClient(SteamGuardSettings settings)
        {
            HttpClient httpClient = HttpClientCreator.Create();

            httpClient.Timeout = TimeSpan.FromSeconds(settings.NetworkTimeoutSeconds);

            return httpClient;
        }

        private static string ComposeDataDirectoryPath() => System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ApplicationDataDirectoryName);

        private static string ComposeKeySeed(string dataDirectory) => string.Join(
            ':',
            Environment.MachineName,
            Environment.UserName,
            dataDirectory);
    }
}
