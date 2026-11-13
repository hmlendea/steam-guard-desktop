using System;

namespace SteamGuardDesktop.Domain.Confirmations
{
    /// <summary>
    /// Represents a decision that must be applied to a specific pending confirmation.
    /// </summary>
    public sealed class ConfirmationDecision : IEquatable<ConfirmationDecision>
    {
        /// <summary>
        /// Gets or sets the identifier of the confirmation to act upon.
        /// </summary>
        public string ConfirmationIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the nonce of the confirmation to act upon.
        /// </summary>
        public string Nonce { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the decision to apply.
        /// </summary>
        public ConfirmationDecisionType DecisionType { get; set; }

        /// <summary>
        /// Gets the operation name expected by the Steam confirmation endpoint.
        /// </summary>
        public string OperationName => GetOperationName(DecisionType);

        /// <summary>
        /// Maps a decision type to the operation name expected by Steam.
        /// </summary>
        /// <param name="decisionType">The decision type to map.</param>
        /// <returns>The Steam operation name.</returns>
        public static string GetOperationName(ConfirmationDecisionType decisionType)
        {
            if (decisionType == ConfirmationDecisionType.Approve)
            {
                return "allow";
            }

            return "cancel";
        }

        /// <inheritdoc />
        public bool Equals(ConfirmationDecision? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(ConfirmationIdentifier, other.ConfirmationIdentifier, StringComparison.Ordinal) &&
                DecisionType == other.DecisionType;
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

            return Equals((ConfirmationDecision)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(ConfirmationIdentifier, DecisionType);

        /// <inheritdoc />
        public override string ToString() => $"{DecisionType}:{ConfirmationIdentifier}";
    }
}
