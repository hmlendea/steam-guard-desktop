using System;

using SteamGuardDesktop.Service.Time;

namespace SteamGuardDesktop.UnitTests.Utils
{
    /// <summary>
    /// Provides a clock whose current moment is supplied by the test.
    /// </summary>
    internal sealed class FixedClock(DateTimeOffset moment) : IClock
    {
        /// <inheritdoc />
        public DateTimeOffset GetUtcNow() => moment;
    }
}