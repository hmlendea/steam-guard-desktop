namespace SteamGuardDesktop.Service.Theming
{
    /// <summary>
    /// Resolves the platform theme that matches a host environment.
    /// </summary>
    public interface IPlatformThemeResolver
    {
        /// <summary>
        /// Resolves the platform theme for the specified host description.
        /// </summary>
        /// <param name="description">The host platform and desktop session description.</param>
        /// <returns>The resolved platform theme.</returns>
        PlatformTheme Resolve(PlatformDescription description);
    }
}
