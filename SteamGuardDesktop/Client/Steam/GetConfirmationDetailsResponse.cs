using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Represents the response of the Steam confirmation details endpoint.
    /// </summary>
    public sealed class GetConfirmationDetailsResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether Steam accepted the request.
        /// </summary>
        [JsonPropertyName("success")]
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the HTML fragment that describes the confirmation.
        /// </summary>
        [JsonPropertyName("html")]
        public string Html { get; set; } = string.Empty;
    }
}
