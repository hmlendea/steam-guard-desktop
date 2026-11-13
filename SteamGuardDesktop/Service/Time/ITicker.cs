using System;

namespace SteamGuardDesktop.Service.Time
{
    /// <summary>
    /// Signals the passing of time so that the interface can refresh without blocking.
    /// </summary>
    public interface ITicker
    {
        /// <summary>
        /// Occurs once per configured interval.
        /// </summary>
        event EventHandler? Tick;

        /// <summary>
        /// Starts signalling at the configured interval.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops signalling.
        /// </summary>
        void Stop();
    }
}
