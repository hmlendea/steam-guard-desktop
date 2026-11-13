using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace SteamGuardDesktop.Service.Confirmations
{
    /// <summary>
    /// Produces confirmation signatures with HMAC-SHA1 over the big-endian timestamp and the request tag.
    /// </summary>
    public sealed class ConfirmationRequestSigner : IConfirmationRequestSigner
    {
        /// <inheritdoc />
        public string GenerateSignature(string identitySecret, long unixTimeSeconds, string tag)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(identitySecret, nameof(identitySecret));
            ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tag));

            byte[] secretBytes = Convert.FromBase64String(identitySecret);
            byte[] timestampBytes = new byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(timestampBytes, unixTimeSeconds);

            byte[] messageBytes = new byte[timestampBytes.Length + Encoding.UTF8.GetByteCount(tag)];
            timestampBytes.CopyTo(messageBytes, 0);
            Encoding.UTF8.GetBytes(tag).CopyTo(messageBytes, timestampBytes.Length);

            using HMACSHA1 hmac = new(secretBytes);

            return Convert.ToBase64String(hmac.ComputeHash(messageBytes));
        }
    }
}
