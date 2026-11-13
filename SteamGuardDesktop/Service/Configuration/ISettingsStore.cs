namespace SteamGuardDesktop.Service.Configuration
{
    /// <summary>
    /// Persists the user settings of the application between runs.
    /// </summary>
    public interface ISettingsStore
    {
        /// <summary>
        /// Loads the stored settings, or the defaults when nothing has been stored.
        /// </summary>
        /// <returns>The stored settings.</returns>
        SettingsSnapshot Load();

        /// <summary>
        /// Saves the supplied settings.
        /// </summary>
        /// <param name="snapshot">The settings to save.</param>
        void Save(SettingsSnapshot snapshot);
    }
}
