using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Deserialises Steam response payloads with a strict, reused serializer configuration.
    /// </summary>
    public sealed class SteamResponseParser : ISteamResponseParser
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = false,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
        };

        /// <inheritdoc />
        public bool TryParse<TValue>(string payload, out TValue? value)
            where TValue : class
        {
            value = null;

            if (string.IsNullOrWhiteSpace(payload))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<TValue>(payload, SerializerOptions);

                return value is not null;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
