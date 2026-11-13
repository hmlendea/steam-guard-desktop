using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Stores named secrets in an encrypted file that is used when no platform credential store is available.
    /// </summary>
    /// <remarks>
    /// The payload is encrypted with AES-256-GCM. The encryption key is derived from a machine-protected seed, so
    /// copying the file alone is insufficient to recover the stored secrets.
    /// </remarks>
    public sealed class EncryptedFileSecretStore : ISecretStore
    {
        private static int NonceSizeBytes => 12;

        private static int TagSizeBytes => 16;

        private static string KeySeedPurpose => "SteamGuardDesktop.SecretStore.KeyDerivation";

        private readonly string filePath;

        private readonly byte[] encryptionKey;

        private readonly Dictionary<string, string> entries;

        /// <summary>
        /// Initialises a new instance of the <see cref="EncryptedFileSecretStore"/> class.
        /// </summary>
        /// <param name="filePath">The path of the encrypted store file.</param>
        /// <param name="keySeed">The machine-protected seed used to derive the encryption key.</param>
        public EncryptedFileSecretStore(string filePath, string keySeed)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));
            ArgumentException.ThrowIfNullOrWhiteSpace(keySeed, nameof(keySeed));

            this.filePath = filePath;
            encryptionKey = DeriveKey(keySeed);
            entries = LoadEntries();
        }

        /// <inheritdoc />
        public SecretStoreAvailability GetAvailability() => SecretStoreAvailability.Available;

        /// <inheritdoc />
        public SecretStoreResult Store(string entryName, string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryName, nameof(entryName));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            entries[entryName] = value;
            PersistEntries();

            return SecretStoreResult.Successful();
        }

        /// <inheritdoc />
        public string? Retrieve(string entryName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryName, nameof(entryName));

            if (!entries.TryGetValue(entryName, out string? value))
            {
                return null;
            }

            return value;
        }

        /// <inheritdoc />
        public SecretStoreResult Remove(string entryName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryName, nameof(entryName));

            entries.Remove(entryName);
            PersistEntries();

            return SecretStoreResult.Successful();
        }

        private static byte[] DeriveKey(string keySeed)
        {
            byte[] seedBytes = Encoding.UTF8.GetBytes(keySeed + KeySeedPurpose);

            return SHA256.HashData(seedBytes);
        }

        private static Dictionary<string, string> DeserialiseEntries(byte[] plainText)
        {
            Dictionary<string, string> loadedEntries = [];
            string content = Encoding.UTF8.GetString(plainText);

            foreach (string line in content.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                int separatorIndex = line.IndexOf(' ', StringComparison.Ordinal);

                if (separatorIndex <= 0)
                {
                    continue;
                }

                string entryName = line[..separatorIndex];
                string entryValue = Encoding.UTF8.GetString(Convert.FromBase64String(line[(separatorIndex + 1)..]));

                loadedEntries[entryName] = entryValue;
            }

            return loadedEntries;
        }

        private Dictionary<string, string> LoadEntries()
        {
            if (!File.Exists(filePath))
            {
                return [];
            }

            byte[] payload = File.ReadAllBytes(filePath);

            if (payload.Length == 0)
            {
                return [];
            }

            if (payload.Length <= NonceSizeBytes + TagSizeBytes)
            {
                throw new CryptographicException(
                    $"The encrypted secret file at {filePath} is too short to contain a valid payload.");
            }

            byte[] nonce = payload[..NonceSizeBytes];
            byte[] tag = payload[NonceSizeBytes..(NonceSizeBytes + TagSizeBytes)];
            byte[] cipherText = payload[(NonceSizeBytes + TagSizeBytes)..];
            byte[] plainText = new byte[cipherText.Length];

            using AesGcm aesGcm = new(encryptionKey, TagSizeBytes);
            aesGcm.Decrypt(nonce, cipherText, tag, plainText);

            return DeserialiseEntries(plainText);
        }

        private void PersistEntries()
        {
            byte[] plainText = SerialiseEntries();
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
            byte[] cipherText = new byte[plainText.Length];
            byte[] tag = new byte[TagSizeBytes];

            using AesGcm aesGcm = new(encryptionKey, TagSizeBytes);
            aesGcm.Encrypt(nonce, plainText, cipherText, tag);

            byte[] payload = new byte[NonceSizeBytes + TagSizeBytes + cipherText.Length];
            nonce.CopyTo(payload, 0);
            tag.CopyTo(payload, NonceSizeBytes);
            cipherText.CopyTo(payload, NonceSizeBytes + TagSizeBytes);

            string? directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(filePath, payload);
        }

        private byte[] SerialiseEntries()
        {
            StringBuilder builder = new();

            foreach (KeyValuePair<string, string> entry in entries)
            {
                builder
                    .Append(entry.Key)
                    .Append(' ')
                    .Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(entry.Value)))
                    .Append('\n');
            }

            return Encoding.UTF8.GetBytes(builder.ToString());
        }
    }
}
