using System;

using CommunityToolkit.Mvvm.ComponentModel;

using SteamGuardDesktop.Configuration;
using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Domain.Authenticator;

namespace SteamGuardDesktop.ViewModels
{
    /// <summary>
    /// Presents the first-run and account editing form.
    /// </summary>
    public sealed partial class AccountSetupViewModel : ObservableObject
    {
        private readonly SteamGuardSettings settings;

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

        [ObservableProperty]
        private bool areSecretsRevealed;

        /// <summary>
        /// Initialises a new instance of the <see cref="AccountSetupViewModel"/> class.
        /// </summary>
        /// <param name="settings">The application settings.</param>
        public AccountSetupViewModel(SteamGuardSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

            this.settings = settings;
        }

        /// <summary>
        /// Gets the character that conceals a secret when it is not revealed.
        /// </summary>
        public char SecretMaskCharacter => AreSecretsRevealed ? '\0' : '*';

        /// <summary>
        /// Gets a value indicating whether the reveal action is available.
        /// </summary>
        public bool IsSecretRevealAllowed => settings.IsSecretRevealAllowed;

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
            AreSecretsRevealed = false;
        }

        /// <summary>
        /// Reports that the reveal preference has changed.
        /// </summary>
        public void NotifySecretRevealAvailabilityChanged()
        {
            if (!settings.IsSecretRevealAllowed)
            {
                AreSecretsRevealed = false;
            }

            OnPropertyChanged(nameof(IsSecretRevealAllowed));
        }

        /// <summary>
        /// Toggles the visibility of the entered secrets when the reveal preference allows it.
        /// </summary>
        public void ToggleSecretVisibility()
        {
            if (!settings.IsSecretRevealAllowed)
            {
                return;
            }

            AreSecretsRevealed = !AreSecretsRevealed;
        }

        /// <inheritdoc />
        partial void OnAreSecretsRevealedChanged(bool value) => OnPropertyChanged(nameof(SecretMaskCharacter));
    }
}
