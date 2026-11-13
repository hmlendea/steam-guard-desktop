using System;
using System.Globalization;
using System.Security.Cryptography;

namespace SteamGuardDesktop.Service.Login
{
    /// <summary>
    /// Encrypts account passwords with the RSA public key published by Steam.
    /// </summary>
    public sealed class PasswordEncryptor : IPasswordEncryptor
    {
        private static int DefaultExponent => 65537;

        /// <inheritdoc />
        public string Encrypt(string publicKeyModulus, string publicKeyExponent, string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(publicKeyModulus, nameof(publicKeyModulus));
            ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));

            byte[] modulus = ParsePublicKeyComponent(publicKeyModulus, "The RSA public key modulus");
            int exponent = ParseExponent(publicKeyExponent);

            using RSA rsa = RSA.Create();
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = modulus,
                Exponent = ConvertExponent(exponent)
            });

            byte[] encryptedPassword = rsa.Encrypt(
                System.Text.Encoding.UTF8.GetBytes(password),
                RSAEncryptionPadding.Pkcs1);

            return Convert.ToBase64String(encryptedPassword);
        }

        private static byte[] ConvertExponent(int exponent)
        {
            byte[] exponentBytes = BitConverter.GetBytes(exponent);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(exponentBytes);
            }

            int firstSignificantIndex = Array.FindIndex(exponentBytes, value => value != 0);

            if (firstSignificantIndex < 0)
            {
                return [0];
            }

            return exponentBytes[firstSignificantIndex..];
        }

        private static int ParseExponent(string publicKeyExponent)
        {
            if (string.IsNullOrWhiteSpace(publicKeyExponent))
            {
                return DefaultExponent;
            }

            if (!int.TryParse(
                publicKeyExponent.Trim(),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out int exponent))
            {
                return DefaultExponent;
            }

            return exponent;
        }

        private static byte[] ParsePublicKeyComponent(string component, string description)
        {
            string normalisedComponent = component.Trim();

            if (normalisedComponent.Length % 2 == 0)
            {
                try
                {
                    byte[] hexadecimalBytes = Convert.FromHexString(normalisedComponent);

                    if (hexadecimalBytes.Length > 0)
                    {
                        return hexadecimalBytes;
                    }
                }
                catch (FormatException)
                {
                    // The component is not hexadecimal, so it is decoded as Base64 below.
                }
            }

            try
            {
                return Convert.FromBase64String(normalisedComponent);
            }
            catch (FormatException exception)
            {
                throw new ArgumentException(
                    $"{description} is neither a hexadecimal nor a Base64 value.",
                    nameof(component),
                    exception);
            }
        }
    }
}
