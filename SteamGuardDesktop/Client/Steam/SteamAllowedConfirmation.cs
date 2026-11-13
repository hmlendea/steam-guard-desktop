using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Represents a guard that Steam accepts for a pending sign-in.
    /// </summary>
    public sealed class SteamAllowedConfirmation
    {
        /// <summary>
        /// Gets or sets the numerical guard type.
        /// </summary>
        [JsonPropertyName("confirmation_type")]
        public int ConfirmationType { get; set; }

        /// <summary>
        /// Gets or sets the guard-specific message reported by Steam.
        /// </summary>
        [JsonPropertyName("associated_message")]
        public string? AssociatedMessage { get; set; }
    }
}
