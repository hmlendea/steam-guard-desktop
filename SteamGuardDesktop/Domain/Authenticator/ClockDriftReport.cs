using System;

namespace SteamGuardDesktop.Domain.Authenticator
{
    /// <summary>
    /// Represents the result of comparing the local clock with the Steam clock.
    /// </summary>
    public sealed class ClockDriftReport
    {
        /// <summary>
        /// Gets or sets a value indicating whether the local clock agrees with the Steam clock.
        /// </summary>
        public bool IsSynchronised { get; set; } = true;

        /// <summary>
        /// Gets or sets the absolute offset between the local clock and the Steam clock.
        /// </summary>
        public TimeSpan Offset { get; set; }

        /// <summary>
        /// Gets a value indicating whether the report requires user attention.
        /// </summary>
        public bool RequiresAttention => !IsSynchronised;
    }
}
