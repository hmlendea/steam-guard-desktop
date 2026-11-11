using System;

namespace SteamGuardDesktop.Service.Time
{
    /// <summary>
    /// Provides the current time from the operating system clock.
    /// </summary>
    public sealed class SystemClock : IClock
    {
        /// <inheritdoc />
        public DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;
    }
}
