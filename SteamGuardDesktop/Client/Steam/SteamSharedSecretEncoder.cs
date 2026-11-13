using System;
using System.Text;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Converts a shared secret from the byte form that Steam returns into the Base32 form that the authenticator
    /// consumes.
    /// </summary>
    /// <remarks>
    /// Steam reports the shared secret as a Base64-encoded byte sequence, while the code generator and every stored
    /// account expect the Base32 representation, so the conversion happens at the protocol boundary.
    /// </remarks>
    public static class SteamSharedSecretEncoder
    {
        private static string Alphabet => "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        private static int BitsPerCharacter => 5;

        private static int BitsPerByte => 8;

        /// <summary>
        /// Encodes the specified bytes as an unpadded Base32 value.
        /// </summary>
        /// <param name="value">The bytes of the shared secret.</param>
        /// <returns>The Base32 representation of the bytes.</returns>
        public static string Encode(byte[] value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            if (value.Length == 0)
            {
                return string.Empty;
            }

            string alphabet = Alphabet;
            StringBuilder builder = new((value.Length * BitsPerByte / BitsPerCharacter) + 1);
            int buffer = 0;
            int bufferedBitCount = 0;

            foreach (byte currentByte in value)
            {
                buffer = (buffer << BitsPerByte) | currentByte;
                bufferedBitCount += BitsPerByte;

                while (bufferedBitCount >= BitsPerCharacter)
                {
                    bufferedBitCount -= BitsPerCharacter;
                    builder.Append(alphabet[(buffer >> bufferedBitCount) & 31]);
                }
            }

            if (bufferedBitCount > 0)
            {
                builder.Append(alphabet[(buffer << (BitsPerCharacter - bufferedBitCount)) & 31]);
            }

            return builder.ToString();
        }
    }
}
