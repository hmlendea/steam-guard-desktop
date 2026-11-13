using System;

namespace SteamGuardDesktop.Domain.Login
{
    /// <summary>
    /// Represents the tokens issued by Steam after a successful sign-in.
    /// </summary>
    public sealed class LoginTokens : IEquatable<LoginTokens>
    {
        /// <summary>
        /// Gets or sets the 64-bit Steam identifier of the authenticated account.
        /// </summary>
        public string SteamIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the access token used by the confirmation endpoints. This value is sensitive.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the refresh token used to obtain a new access token. This value is sensitive.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether an access token was issued.
        /// </summary>
        public bool HasAccessToken => !string.IsNullOrWhiteSpace(AccessToken);

        /// <summary>
        /// Gets a value indicating whether a refresh token was issued.
        /// </summary>
        public bool HasRefreshToken => !string.IsNullOrWhiteSpace(RefreshToken);

        /// <inheritdoc />
        public bool Equals(LoginTokens? other)
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

            return Equals((LoginTokens)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(SteamIdentifier, AccessToken);

        /// <inheritdoc />
        public override string ToString() => $"LoginTokens({SteamIdentifier})";
    }
}
