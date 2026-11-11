using System;

using SteamGuardDesktop.Client.Steam;
using SteamGuardDesktop.Domain.Authenticator;

namespace SteamGuardDesktop.Service.Time
{
    /// <summary>
    /// Compares the local clock with the clock reported by the Steam servers.
    /// </summary>
    public sealed class ClockDriftService(
        ISteamConfirmationClient steamClient,
        IClock clock) : IClockDriftService
    {
        private static TimeSpan AcceptableDrift => TimeSpan.FromSeconds(30);

        /// <inheritdoc />
        public ClockDriftReport? Evaluate()
        {
            SteamClientResult<long> serverTimeResult = steamClient.GetServerTime();

            if (!serverTimeResult.IsSuccessful)
            {
                return null;
            }

            DateTimeOffset steamMoment = DateTimeOffset.FromUnixTimeSeconds(serverTimeResult.Value);
            TimeSpan offset = clock.GetUtcNow() - steamMoment;

            return new ClockDriftReport
            {
                IsSynchronised = offset.Duration() <= AcceptableDrift,
                Offset = offset
            };
        }
    }
}
