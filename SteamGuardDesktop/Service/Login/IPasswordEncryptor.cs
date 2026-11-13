using System;

namespace SteamGuardDesktop.Service.Login
{
    /// <summary>
    /// Encrypts account passwords with the RSA public key published by Steam.
    /// </summary>
    public interface IPasswordEncryptor
    {
        /// <summary>
        /// Encrypts a password with the specified RSA public key.
        /// </summary>
        /// <param name="publicKeyModulus">The Base64-or-hexadecimal RSA public key modulus published by Steam.</param>
        /// <param name="publicKeyExponent">The hexadecimal RSA public key exponent published by Steam.</param>
        /// <param name="password">The password to encrypt. This value is never persisted.</param>
        /// <returns>The encrypted password encoded as Base64.</returns>
        string Encrypt(string publicKeyModulus, string publicKeyExponent, string password);
    }
}
