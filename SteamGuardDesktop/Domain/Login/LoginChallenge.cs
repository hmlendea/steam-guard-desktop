using System;
using System.Collections.Generic;

namespace SteamGuardDesktop.Domain.Login
{
    /// <summary>
    /// Represents an authentication attempt that Steam is waiting to be confirmed.
    /// </summary>
    public sealed class LoginChallenge : IEquatable<LoginChallenge>
    {
        /// <summary>
        /// Gets or sets the identifier that Steam uses to route the authentication attempt.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the opaque request value that must accompany every poll.
        /// </summary>
        public string RequestId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the interval, in seconds, at which Steam expects the attempt to be polled.
        /// </summary>
        public double PollingIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the 64-bit Steam identifier, when Steam supplied it.
        /// </summary>
        public string SteamIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the guards that Steam accepts for this attempt.
        /// </summary>
        public IEnumerable<LoginGuardType> AcceptedGuards { get; set; } = [];

        /// <summary>
        /// Gets or sets the guard-specific message reported by Steam, such as a partially redacted address.
        /// </summary>
        public string GuardMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message reported by Steam when the credentials were rejected.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <inheritdoc />
        public bool Equals(LoginChallenge? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(ClientId, other.ClientId, StringComparison.Ordinal);
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

            return Equals((LoginChallenge)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(ClientId, SteamIdentifier);

        /// <inheritdoc />
        public override string ToString() => $"LoginChallenge({SteamIdentifier})";
    }
}
