using System;
using System.Text;

using SteamGuard.TOTP;
using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Service.Time;

namespace SteamGuardDesktop.Service.Authenticator
{
    /// <summary>
    /// Generates Steam Guard authentication codes from a shared secret.
    /// </summary>
    public interface IOtpCodeGenerator
    {
        /// <summary>
        /// Generates the authentication code that is valid for a shared secret at the current moment.
        /// </summary>
        /// <param name="sharedSecret">The Base32 shared secret of the account.</param>
        /// <returns>The generated code, or <c>null</c> when the secret cannot be used.</returns>
        SteamGuardCode? GenerateCode(string sharedSecret);

        /// <summary>
        /// Validates a shared secret without revealing it.
        /// </summary>
        /// <param name="sharedSecret">The Base32 shared secret to validate.</param>
        /// <returns><c>true</c> when the secret can be used to generate codes.</returns>
        bool IsValidSharedSecret(string sharedSecret);
    }
}
