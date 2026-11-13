using System;
using System.Text;

using SteamGuard.TOTP;
using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Service.Time;

namespace SteamGuardDesktop.Service.Authenticator
{
    /// <summary>
    /// Generates Steam Guard authentication codes using the SteamGuard.TOTP package.
    /// </summary>
    public sealed class OtpCodeGenerator : IOtpCodeGenerator
    {
        private static TimeSpan CodeValidityWindow => TimeSpan.FromSeconds(30);

        private static string AllowedSecretCharacters => "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        private readonly ISteamGuard steamGuard;

        private readonly IClock clock;

        /// <summary>
        /// Initialises a new instance of the <see cref="OtpCodeGenerator"/> class.
        /// </summary>
        /// <param name="steamGuard">The authenticator used to compute codes.</param>
        /// <param name="clock">The clock used to determine code validity.</param>
        public OtpCodeGenerator(ISteamGuard steamGuard, IClock clock)
        {
            ArgumentNullException.ThrowIfNull(steamGuard, nameof(steamGuard));
            ArgumentNullException.ThrowIfNull(clock, nameof(clock));

            this.steamGuard = steamGuard;
            this.clock = clock;
        }

        /// <inheritdoc />
        public SteamGuardCode? GenerateCode(string sharedSecret)
        {
            if (!IsValidSharedSecret(sharedSecret))
            {
                return null;
            }

            DateTimeOffset generatedAt = clock.GetUtcNow();
            string code = steamGuard.GenerateAuthenticationCode(sharedSecret);

            return new SteamGuardCode
            {
                Value = code,
                GeneratedAt = generatedAt,
                ExpiresAt = GetExpiry(generatedAt)
            };
        }

        /// <inheritdoc />
        public bool IsValidSharedSecret(string sharedSecret)
        {
            if (string.IsNullOrWhiteSpace(sharedSecret))
            {
                return false;
            }

            string normalisedSecret = sharedSecret.Trim().TrimEnd('=').ToUpperInvariant();

            if (normalisedSecret.Length == 0)
            {
                return false;
            }

            foreach (char secretCharacter in normalisedSecret)
            {
                if (AllowedSecretCharacters.IndexOf(secretCharacter, StringComparison.Ordinal) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static DateTimeOffset GetExpiry(DateTimeOffset generatedAt)
        {
            long elapsedTicks = generatedAt.UtcTicks;
            long windowTicks = CodeValidityWindow.Ticks;
            long remainingTicks = windowTicks - (elapsedTicks % windowTicks);

            return generatedAt.AddTicks(remainingTicks);
        }
    }
}
