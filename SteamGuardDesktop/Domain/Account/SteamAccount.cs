using System;

namespace SteamGuardDesktop.Domain.Account
{
    /// <summary>
    /// Represents the non-secret metadata of a Steam account that uses Steam Guard.
    /// </summary>
    public sealed class SteamAccount : IEquatable<SteamAccount>
    {
        /// <summary>
        /// Gets or sets the locally generated account identifier.
        /// </summary>
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user-facing account label.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the 64-bit Steam identifier.
        /// </summary>
        public string SteamIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the device identifier reported to Steam.
        /// </summary>
        public string DeviceIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the credential store entry name that holds the shared secret.
        /// </summary>
        public string SharedSecretName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the credential store entry name that holds the identity secret.
        /// </summary>
        public string IdentitySecretName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the credential store entry name that holds the revocation code.
        /// </summary>
        public string RevocationCodeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the credential store entry name that holds the Steam access token.
        /// </summary>
        public string AccessTokenName { get; set; } = string.Empty;

        /// <inheritdoc />
        public bool Equals(SteamAccount? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Identifier, other.Identifier, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((SteamAccount)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Identifier, SteamIdentifier);

        /// <inheritdoc />
        public override string ToString() => Label;
    }
}
