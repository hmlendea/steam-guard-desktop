using System;

namespace SteamGuardDesktop.Domain.Confirmations
{
    /// <summary>
    /// Represents a pending Steam Guard confirmation without any secret material.
    /// </summary>
    public sealed class Confirmation : IEquatable<Confirmation>
    {
        /// <summary>
        /// Gets or sets the confirmation identifier assigned by Steam.
        /// </summary>
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the confirmation nonce required to apply a decision.
        /// </summary>
        public string Nonce { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the interpreted confirmation type.
        /// </summary>
        public ConfirmationType Type { get; set; }

        /// <summary>
        /// Gets or sets the confirmation type name reported by Steam.
        /// </summary>
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the object affected by the confirmation.
        /// </summary>
        public string CreatorIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the headline displayed to the user.
        /// </summary>
        public string Headline { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the additional safe context displayed to the user.
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the moment at which the confirmation was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets a value indicating whether a decision can be applied to the confirmation.
        /// </summary>
        public bool CanBeDecided => Type != ConfirmationType.Unsupported;

        /// <inheritdoc />
        public bool Equals(Confirmation? other)
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

            return Equals((Confirmation)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Identifier, Nonce);

        /// <inheritdoc />
        public override string ToString() => Headline;
    }
}
