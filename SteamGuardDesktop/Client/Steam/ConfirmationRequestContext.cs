using SteamGuardDesktop.Domain.Steam;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Provides the account material required to call the Steam confirmation endpoints.
    /// </summary>
    public sealed class ConfirmationRequestContext
    {
        /// <summary>
        /// Gets or sets the authenticated Steam session.
        /// </summary>
        public SteamSession Session { get; set; } = new();

        /// <summary>
        /// Gets or sets the device identifier reported to Steam.
        /// </summary>
        public string DeviceIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Base64 identity secret used to sign requests. This value is sensitive.
        /// </summary>
        public string IdentitySecret { get; set; } = string.Empty;
    }
}
