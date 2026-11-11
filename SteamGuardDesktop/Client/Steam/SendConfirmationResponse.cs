using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Represents the response of the Steam confirmation decision endpoint.
    /// </summary>
    public sealed class SendConfirmationResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether Steam accepted the decision.
        /// </summary>
        [JsonPropertyName("success")]
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request requires a new authenticated session.
        /// </summary>
        [JsonPropertyName("needsauth")]
        public bool? RequiresAuthentication { get; set; }

        /// <summary>
        /// Gets or sets the safe message reported by Steam.
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
