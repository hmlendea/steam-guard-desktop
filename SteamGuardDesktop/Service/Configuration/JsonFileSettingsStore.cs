using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Service.Configuration
{
    /// <summary>
    /// Persists the user settings in a JSON file under the user profile.
    /// </summary>
    /// <remarks>
    /// The file contains no secrets; every secret is stored through the credential store instead.
    /// </remarks>
    public sealed class JsonFileSettingsStore : ISettingsStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        private readonly string filePath;

        /// <summary>
        /// Initialises a new instance of the <see cref="JsonFileSettingsStore"/> class.
        /// </summary>
        /// <param name="filePath">The path of the settings file.</param>
        public JsonFileSettingsStore(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

            this.filePath = filePath;
        }

        /// <inheritdoc />
        public SettingsSnapshot Load()
        {
            if (!File.Exists(filePath))
            {
                return new SettingsSnapshot();
            }

            try
            {
                string content = File.ReadAllText(filePath);

                return JsonSerializer.Deserialize<SettingsSnapshot>(content, SerializerOptions)
                    ?? new SettingsSnapshot();
            }
            catch (JsonException)
            {
                return new SettingsSnapshot();
            }
            catch (IOException)
            {
                return new SettingsSnapshot();
            }
        }

        /// <inheritdoc />
        public void Save(SettingsSnapshot snapshot)
        {
            ArgumentNullException.ThrowIfNull(snapshot, nameof(snapshot));

            string? directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, JsonSerializer.Serialize(snapshot, SerializerOptions));
        }
    }
}
