using System;

using CommunityToolkit.Mvvm.ComponentModel;

using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Service.Authenticator;
using SteamGuardDesktop.Service.Time;

namespace SteamGuardDesktop.ViewModels
{
    /// <summary>
    /// Presents the current authentication code and its remaining validity.
    /// </summary>
    public sealed partial class AuthenticatorViewModel : ViewModelBase
    {
        private static TimeSpan CodeLifetime => TimeSpan.FromSeconds(30);

        private readonly IOtpCodeGenerator otpCodeGenerator;

        private readonly IClock clock;

        [ObservableProperty]
        private string currentCode = string.Empty;

        [ObservableProperty]
        private int remainingSeconds;

        [ObservableProperty]
        private bool hasClockWarning;

        [ObservableProperty]
        private bool hasSecretProblem;

        /// <summary>
        /// Initialises a new instance of the <see cref="AuthenticatorViewModel"/> class.
        /// </summary>
        /// <param name="otpCodeGenerator">The generator used to compute codes.</param>
        /// <param name="clock">The clock used to determine the remaining validity.</param>
        public AuthenticatorViewModel(IOtpCodeGenerator otpCodeGenerator, IClock clock)
        {
            ArgumentNullException.ThrowIfNull(otpCodeGenerator, nameof(otpCodeGenerator));
            ArgumentNullException.ThrowIfNull(clock, nameof(clock));

            this.otpCodeGenerator = otpCodeGenerator;
            this.clock = clock;
        }

        /// <summary>
        /// Gets a value indicating whether a code is currently displayed.
        /// </summary>
        public bool HasCode => !string.IsNullOrWhiteSpace(CurrentCode);

        /// <summary>
        /// Generates a code for the specified shared secret.
        /// </summary>
        /// <param name="sharedSecret">The Base32 shared secret of the account.</param>
        public void Refresh(string sharedSecret)
        {
            SteamGuardCode? code = otpCodeGenerator.GenerateCode(sharedSecret);

            if (code is null)
            {
                CurrentCode = string.Empty;
                RemainingSeconds = 0;
                HasSecretProblem = true;
                OnPropertyChanged(nameof(HasCode));

                return;
            }

            CurrentCode = code.Value;
            HasSecretProblem = false;
            UpdateCountdown(code.ExpiresAt);
            OnPropertyChanged(nameof(HasCode));
        }

        /// <summary>
        /// Updates the remaining validity of the displayed code.
        /// </summary>
        /// <param name="expiry">The moment at which the displayed code expires.</param>
        public void UpdateCountdown(DateTimeOffset expiry)
        {
            TimeSpan remaining = expiry - clock.GetUtcNow();
            RemainingSeconds = remaining.TotalSeconds > 0 ? (int)Math.Ceiling(remaining.TotalSeconds) : 0;
        }

        /// <summary>
        /// Clears the currently displayed code.
        /// </summary>
        public void Clear()
        {
            CurrentCode = string.Empty;
            RemainingSeconds = 0;
            HasClockWarning = false;
            HasSecretProblem = false;
            OnPropertyChanged(nameof(HasCode));
        }

        /// <summary>
        /// Applies the result of a clock comparison.
        /// </summary>
        /// <param name="driftReport">The clock drift report.</param>
        public void ApplyClockDrift(ClockDriftReport driftReport)
        {
            ArgumentNullException.ThrowIfNull(driftReport, nameof(driftReport));

            HasClockWarning = driftReport.RequiresAttention;
        }

        /// <summary>
        /// Gets the lifetime of a generated code.
        /// </summary>
        public static TimeSpan GetCodeLifetime() => CodeLifetime;

        /// <inheritdoc />
        partial void OnCurrentCodeChanged(string value) => OnPropertyChanged(nameof(HasCode));
    }
}
