using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Represents the response of the Steam server time endpoint.
    /// </summary>
    public sealed class GetServerTimeResponse
    {
        /// <summary>
        /// Gets or sets the payload of the response.
        /// </summary>
        [JsonPropertyName("response")]
        public ServerTimePayload? Response { get; set; }
    }
}
