using System;

namespace SteamGuardDesktop.Domain.Login
{
    /// <summary>
    /// Represents the credentials supplied when starting a sign-in.
    /// </summary>
    public sealed class LoginCredentials
    {
        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account password. This value is sensitive and is never persisted.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the friendly name reported to Steam for this device.
        /// </summary>
        public string DeviceFriendlyName { get; set; } = "Steam Guard Desktop";

        /// <inheritdoc />
        public override string ToString() => $"LoginCredentials({AccountName})";
    }
}
