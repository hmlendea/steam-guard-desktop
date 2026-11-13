using SteamGuardDesktop.Domain.Authenticator;

namespace SteamGuardDesktop.Service.Time
{
    /// <summary>
    /// Compares the local clock with the clock reported by the Steam servers.
    /// </summary>
    public interface IClockDriftService
    {
        /// <summary>
        /// Evaluates the local clock against the Steam clock.
        /// </summary>
        /// <returns>The clock drift report, or <c>null</c> when the Steam clock cannot be read.</returns>
        ClockDriftReport? Evaluate();
    }
}
