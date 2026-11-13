using System;

using Avalonia.Threading;

namespace SteamGuardDesktop.Service.Time
{
    /// <summary>
    /// Signals the passing of time on the user-interface thread.
    /// </summary>
    public sealed class AvaloniaTicker : ITicker
    {
        private static TimeSpan TickInterval => TimeSpan.FromSeconds(1);

        private readonly DispatcherTimer timer;

        /// <summary>
        /// Initialises a new instance of the <see cref="AvaloniaTicker"/> class.
        /// </summary>
        public AvaloniaTicker()
        {
            timer = new DispatcherTimer { Interval = TickInterval };
            timer.Tick += OnTimerTick;
        }

        /// <inheritdoc />
        public event EventHandler? Tick;

        /// <inheritdoc />
        public void Start()
        {
            if (!timer.IsEnabled)
            {
                timer.Start();
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
            }
        }

        private void OnTimerTick(object? sender, EventArgs arguments) => Tick?.Invoke(this, EventArgs.Empty);
    }
}
