using System;

namespace SteamGuardDesktop.Domain.Login
{
    /// <summary>
    /// Represents a freshly enrolled Steam Mobile Authenticator.
    /// </summary>
    public sealed class EnrolledAuthenticator
    {
        /// <summary>
        /// Gets or sets the 64-bit Steam identifier of the account.
        /// </summary>
        public string SteamIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Base32 shared secret of the authenticator. This value is sensitive.
        /// </summary>
        public string SharedSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Base64 identity secret of the authenticator. This value is sensitive.
        /// </summary>
        public string IdentitySecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the revocation code of the authenticator. This value is sensitive.
        /// </summary>
        public string RevocationCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the device identifier that Steam assigned to the authenticator.
        /// </summary>
        public string DeviceIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether every mandatory secret is present.
        /// </summary>
        public bool AreSecretsPresent =>
            !string.IsNullOrWhiteSpace(SharedSecret) &&
            !string.IsNullOrWhiteSpace(IdentitySecret);

        /// <inheritdoc />
        public override string ToString() => $"EnrolledAuthenticator({SteamIdentifier})";
    }
}
