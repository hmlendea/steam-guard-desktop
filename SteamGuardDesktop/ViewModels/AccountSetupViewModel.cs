using System;

using CommunityToolkit.Mvvm.ComponentModel;

using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Domain.Authenticator;

namespace SteamGuardDesktop.ViewModels
{
    /// <summary>
    /// Presents the first-run and account editing form.
    /// </summary>
    public sealed partial class AccountSetupViewModel : ObservableObject
    {
        [ObservableProperty]
        private string steamIdentifier = string.Empty;

        [ObservableProperty]
        private string label = string.Empty;

        [ObservableProperty]
        private string sharedSecret = string.Empty;

        [ObservableProperty]
        private string identitySecret = string.Empty;

        [ObservableProperty]
        private string revocationCode = string.Empty;

        [ObservableProperty]
        private string accessToken = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the entered secrets are currently visible.
        /// </summary>
        public bool AreSecretsVisible { get; private set; }

        /// <summary>
        /// Builds the account metadata described by the form.
        /// </summary>
        /// <returns>The account metadata.</returns>
        public SteamAccount BuildAccount() => new()
        {
            SteamIdentifier = SteamIdentifier.Trim(),
            Label = Label.Trim()
        };

        /// <summary>
        /// Builds the secrets described by the form.
        /// </summary>
        /// <returns>The account secrets.</returns>
        public AuthenticatorSecrets BuildSecrets() => new()
        {
            SharedSecret = SharedSecret.Trim(),
            IdentitySecret = IdentitySecret.Trim(),
            RevocationCode = RevocationCode.Trim(),
            AccessToken = AccessToken.Trim()
        };

        /// <summary>
        /// Clears the form so that secret material is not retained.
        /// </summary>
        public void Reset()
        {
            SteamIdentifier = string.Empty;
            Label = string.Empty;
            SharedSecret = string.Empty;
            IdentitySecret = string.Empty;
            RevocationCode = string.Empty;
            AccessToken = string.Empty;
        }

        /// <summary>
        /// Toggles the visibility of the entered secrets.
        /// </summary>
        public void ToggleSecretVisibility()
        {
            AreSecretsVisible = !AreSecretsVisible;
            OnPropertyChanged(nameof(AreSecretsVisible));
        }
    }
}
