using SteamGuard.TOTP;

namespace SteamGuardDesktop.UnitTests.Utils
{
    /// <summary>
    /// Provides a Steam authenticator whose time step is supplied by the test.
    /// </summary>
    internal sealed class FixedTimeStepProvider(long value) : ITimeStepProvider
    {
        /// <inheritdoc />
        public long GetCurrentTimeStep() => value;
    }
}
