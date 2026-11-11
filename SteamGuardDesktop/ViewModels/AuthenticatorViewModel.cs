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
        /// Gets the moment at which the displayed code expires, or <c>null</c> when no code is displayed.
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; private set; }

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
                ExpiresAt = null;
                OnPropertyChanged(nameof(HasCode));

                return;
            }

            CurrentCode = code.Value;
            HasSecretProblem = false;
            ExpiresAt = code.ExpiresAt;
            UpdateCountdown();
            OnPropertyChanged(nameof(HasCode));
        }

        /// <summary>
        /// Updates the remaining validity of the displayed code.
        /// </summary>
        public void UpdateCountdown()
        {
            if (ExpiresAt is null)
            {
                RemainingSeconds = 0;

                return;
            }

            TimeSpan remaining = ExpiresAt.Value - clock.GetUtcNow();
            RemainingSeconds = remaining.TotalSeconds > 0 ? (int)Math.Ceiling(remaining.TotalSeconds) : 0;
        }

        /// <summary>
        /// Determines whether the displayed code has expired.
        /// </summary>
        /// <returns><c>true</c> when a code is displayed and it has expired.</returns>
        public bool HasExpired()
        {
            if (ExpiresAt is null)
            {
                return false;
            }

            return clock.GetUtcNow() >= ExpiresAt.Value;
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
            ExpiresAt = null;
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

        /// <inheritdoc />
        partial void OnCurrentCodeChanged(string value) => OnPropertyChanged(nameof(HasCode));
    }
}
