using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Represents a single confirmation returned by the Steam confirmation list endpoint.
    /// </summary>
    public sealed class ConfirmationDataObject
    {
        /// <summary>
        /// Gets or sets the numeric confirmation type reported by Steam.
        /// </summary>
        [JsonPropertyName("type")]
        public int Type { get; set; }

        /// <summary>
        /// Gets or sets the confirmation type name reported by Steam.
        /// </summary>
        [JsonPropertyName("type_name")]
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the confirmation identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the object affected by the confirmation.
        /// </summary>
        [JsonPropertyName("creator_id")]
        public string CreatorIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the confirmation nonce.
        /// </summary>
        [JsonPropertyName("nonce")]
        public string Nonce { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the creation time of the confirmation, expressed as a Unix timestamp in seconds.
        /// </summary>
        [JsonPropertyName("creation_time")]
        public long CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the headline displayed to the user.
        /// </summary>
        [JsonPropertyName("headline")]
        public string Headline { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the additional context displayed to the user.
        /// </summary>
        [JsonPropertyName("summary")]
        public string[] Summary { get; set; } = [];
    }
}
