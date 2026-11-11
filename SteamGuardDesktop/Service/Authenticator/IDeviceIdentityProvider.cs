using System;
using System.Security.Cryptography;
using System.Text;

namespace SteamGuardDesktop.Service.Authenticator
{
    /// <summary>
    /// Derives the stable device identifier that Steam expects for a given account.
    /// </summary>
    public interface IDeviceIdentityProvider
    {
        /// <summary>
        /// Derives the device identifier that Steam expects for the specified account.
        /// </summary>
        /// <param name="steamIdentifier">The 64-bit Steam identifier of the account.</param>
        /// <returns>The device identifier.</returns>
        string DeriveDeviceIdentifier(string steamIdentifier);
    }
}
