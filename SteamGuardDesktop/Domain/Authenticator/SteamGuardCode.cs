using System;

namespace SteamGuardDesktop.Domain.Authenticator
{
    /// <summary>
    /// Represents a generated Steam Guard authentication code and its validity window.
    /// </summary>
    public sealed class SteamGuardCode : IEquatable<SteamGuardCode>
    {
        /// <summary>
        /// Gets or sets the code value.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the moment at which the code was generated.
        /// </summary>
        public DateTimeOffset GeneratedAt { get; set; }

        /// <summary>
        /// Gets or sets the moment at which the code expires.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// Gets the number of whole seconds remaining before the code expires.
        /// </summary>
        public int RemainingSeconds
        {
            get
            {
                TimeSpan remaining = ExpiresAt - GeneratedAt;

                if (remaining.TotalSeconds <= 0)
                {
                    return 0;
                }

                return (int)Math.Ceiling(remaining.TotalSeconds);
            }
        }

        /// <summary>
        /// Determines whether the code is still valid at the specified moment.
        /// </summary>
        /// <param name="moment">The moment to evaluate.</param>
        /// <returns><c>true</c> when the code has not expired yet.</returns>
        public bool IsValidAt(DateTimeOffset moment) => moment < ExpiresAt;

        /// <inheritdoc />
        public bool Equals(SteamGuardCode? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Value, other.Value, StringComparison.Ordinal) &&
                GeneratedAt.Equals(other.GeneratedAt);
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

            return Equals((SteamGuardCode)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Value, GeneratedAt);

        /// <inheritdoc />
        public override string ToString() => Value;
    }
}
