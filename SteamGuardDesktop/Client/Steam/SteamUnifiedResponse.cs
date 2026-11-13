using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Represents the payload of a unified Steam endpoint response.
    /// </summary>
    public sealed class SteamUnifiedResponse
    {
        /// <summary>
        /// Gets or sets the numerical result reported by Steam.
        /// </summary>
        [JsonPropertyName("result")]
        public int Result { get; set; }

        /// <summary>
        /// Gets or sets the response payload reported by Steam.
        /// </summary>
        [JsonPropertyName("response")]
        public SteamUnifiedResponsePayload? Payload { get; set; }
    }
}
