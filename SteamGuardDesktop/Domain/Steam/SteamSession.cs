using System;

namespace SteamGuardDesktop.Domain.Steam
{
    /// <summary>
    /// Represents the minimum authenticated Steam session material required by the confirmation endpoints.
    /// </summary>
    public sealed class SteamSession : IEquatable<SteamSession>
    {
        /// <summary>
        /// Gets or sets the 64-bit Steam identifier of the authenticated account.
        /// </summary>
        public string SteamIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the access token issued by Steam. This value is sensitive.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the session contains an identifier and an access token.
        /// </summary>
        public bool IsComplete =>
            !string.IsNullOrWhiteSpace(SteamIdentifier) &&
            !string.IsNullOrWhiteSpace(AccessToken);

        /// <inheritdoc />
        public bool Equals(SteamSession? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(SteamIdentifier, other.SteamIdentifier, StringComparison.Ordinal) &&
                string.Equals(AccessToken, other.AccessToken, StringComparison.Ordinal);
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

            return Equals((SteamSession)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(SteamIdentifier, AccessToken);

        /// <inheritdoc />
        public override string ToString() => $"SteamSession({SteamIdentifier})";
    }
}
