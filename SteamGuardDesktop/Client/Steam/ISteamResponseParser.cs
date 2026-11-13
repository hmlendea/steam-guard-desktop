using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Serialises and deserialises Steam response payloads.
    /// </summary>
    public interface ISteamResponseParser
    {
        /// <summary>
        /// Deserialises a Steam response payload.
        /// </summary>
        /// <typeparam name="TValue">The expected payload type.</typeparam>
        /// <param name="payload">The raw response payload.</param>
        /// <param name="value">The deserialised value, when parsing succeeds.</param>
        /// <returns><c>true</c> when the payload was parsed.</returns>
        bool TryParse<TValue>(string payload, out TValue? value)
            where TValue : class;
    }
}
