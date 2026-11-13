using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Represents the response of the Steam confirmation list endpoint.
    /// </summary>
    public sealed class GetConfirmationListResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether Steam accepted the request.
        /// </summary>
        [JsonPropertyName("success")]
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request requires a new authenticated session.
        /// </summary>
        [JsonPropertyName("needauth")]
        public bool? RequiresAuthentication { get; set; }

        /// <summary>
        /// Gets or sets the safe message reported by Steam.
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the pending confirmations.
        /// </summary>
        [JsonPropertyName("conf")]
        public ConfirmationDataObject[] Confirmations { get; set; } = [];
    }
}
