using System;

namespace SteamGuardDesktop.Domain.Authenticator
{
    /// <summary>
    /// Holds the sensitive secrets required to act as a Steam Mobile Authenticator.
    /// </summary>
    public sealed class AuthenticatorSecrets : IEquatable<AuthenticatorSecrets>
    {
        /// <summary>
        /// Gets or sets the shared secret used to generate authenticator codes.
        /// </summary>
        public string SharedSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identity secret used to sign confirmation requests.
        /// </summary>
        public string IdentitySecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the revocation code that removes the authenticator from the account.
        /// </summary>
        public string RevocationCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Steam access token that authenticates confirmation requests.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether every mandatory secret is present.
        /// </summary>
        public bool AreSecretsPresent =>
            !string.IsNullOrWhiteSpace(SharedSecret) &&
            !string.IsNullOrWhiteSpace(IdentitySecret);

        /// <inheritdoc />
        public bool Equals(AuthenticatorSecrets? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(SharedSecret, other.SharedSecret, StringComparison.Ordinal) &&
                string.Equals(IdentitySecret, other.IdentitySecret, StringComparison.Ordinal);
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

            return Equals((AuthenticatorSecrets)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(SharedSecret, IdentitySecret);

        /// <inheritdoc />
        public override string ToString() => nameof(AuthenticatorSecrets);
    }
}
