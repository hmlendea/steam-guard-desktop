using System;

namespace SteamGuardDesktop.Service.Time
{
    /// <summary>
    /// Provides the current time to services so that expiry behaviour remains deterministic.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the current moment expressed as coordinated universal time.
        /// </summary>
        /// <returns>The current moment.</returns>
        DateTimeOffset GetUtcNow();
    }
}
