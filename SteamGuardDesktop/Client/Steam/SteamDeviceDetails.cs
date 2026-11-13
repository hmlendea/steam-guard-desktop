using System.Text.Json.Serialization;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Represents the device details reported when a sign-in starts.
    /// </summary>
    public sealed class SteamDeviceDetails
    {
        /// <summary>
        /// Gets or sets the friendly name reported for this device.
        /// </summary>
        [JsonPropertyName("device_friendly_name")]
        public string DeviceFriendlyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the platform type reported to Steam.
        /// </summary>
        [JsonPropertyName("platform_type")]
        public int PlatformType { get; set; }

        /// <summary>
        /// Gets or sets the operating system type reported to Steam.
        /// </summary>
        [JsonPropertyName("os_type")]
        public int OsType { get; set; }

        /// <summary>
        /// Gets or sets the gaming device type reported to Steam.
        /// </summary>
        [JsonPropertyName("gaming_device_type")]
        public int GamingDeviceType { get; set; }
    }
}
