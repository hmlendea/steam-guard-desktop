using System;
using System.Security.Cryptography;
using System.Text;

namespace SteamGuardDesktop.Service.Authenticator
{
    /// <summary>
    /// Derives the stable device identifier that Steam expects for a given account.
    /// </summary>
    public sealed class DeviceIdentityProvider : IDeviceIdentityProvider
    {
        private static string DevicePrefix => "android:";

        /// <inheritdoc />
        public string DeriveDeviceIdentifier(string steamIdentifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(steamIdentifier, nameof(steamIdentifier));

            byte[] description = Encoding.UTF8.GetBytes(DevicePrefix + steamIdentifier);
            byte[] digest = SHA1.HashData(description);

            return Convert.ToHexStringLower(digest);
        }
    }
}
