using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Represents the server time payload returned by Steam.
    /// </summary>
    public sealed class ServerTimePayload
    {
        /// <summary>
        /// Gets or sets the server time expressed as a string containing a Unix timestamp in seconds.
        /// </summary>
        [JsonPropertyName("server_time")]
        public string ServerTime { get; set; } = string.Empty;
    }
}
