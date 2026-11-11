using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using SteamGuardDesktop.Domain.Account;

namespace SteamGuardDesktop.DataAccess.Accounts
{
    /// <summary>
    /// Persists the non-secret account metadata in an encrypted configuration file.
    /// </summary>
    public sealed class EncryptedFileAccountStore : IAccountStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = false,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        private static string KeySeedPurpose => "SteamGuardDesktop.AccountStore.KeyDerivation";

        private static int NonceSizeBytes => 12;

        private static int TagSizeBytes => 16;

        private readonly string filePath;

        private readonly byte[] encryptionKey;

        private readonly List<SteamAccount> accounts;

        /// <summary>
        /// Initialises a new instance of the <see cref="EncryptedFileAccountStore"/> class.
        /// </summary>
        /// <param name="filePath">The path of the encrypted metadata file.</param>
        /// <param name="keySeed">The machine-protected seed used to derive the encryption key.</param>
        public EncryptedFileAccountStore(string filePath, string keySeed)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));
            ArgumentException.ThrowIfNullOrWhiteSpace(keySeed, nameof(keySeed));

            this.filePath = filePath;
            encryptionKey = DeriveKey(keySeed);
            accounts = LoadAccounts();
        }

        /// <inheritdoc />
        public IEnumerable<SteamAccount> GetAll() => accounts.ToArray();

        /// <inheritdoc />
        public SteamAccount Save(SteamAccount account)
        {
            ArgumentNullException.ThrowIfNull(account, nameof(account));

            if (string.IsNullOrWhiteSpace(account.Identifier))
            {
                account.Identifier = Guid.NewGuid().ToString("N");
            }

            int existingIndex = accounts.FindIndex(candidate => string.Equals(candidate.Identifier, account.Identifier, StringComparison.Ordinal));

            if (existingIndex < 0)
            {
                accounts.Add(account);
            }
            else
            {
                accounts[existingIndex] = account;
            }

            PersistAccounts();

            return account;
        }

        /// <inheritdoc />
        public bool Remove(string accountIdentifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accountIdentifier, nameof(accountIdentifier));

            int removedCount = accounts.RemoveAll(candidate =>
                string.Equals(candidate.Identifier, accountIdentifier, StringComparison.Ordinal));

            if (removedCount == 0)
            {
                return false;
            }

            PersistAccounts();

            return true;
        }

        /// <inheritdoc />
        public bool Contains(string accountIdentifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accountIdentifier, nameof(accountIdentifier));

            return accounts.Any(candidate => string.Equals(candidate.Identifier, accountIdentifier, StringComparison.Ordinal));
        }

        /// <inheritdoc />
        public void Clear()
        {
            accounts.Clear();
            PersistAccounts();
        }

        private static byte[] DeriveKey(string keySeed)
        {
            byte[] seedBytes = Encoding.UTF8.GetBytes(keySeed + KeySeedPurpose);

            return SHA256.HashData(seedBytes);
        }

        private List<SteamAccount> LoadAccounts()
        {
            if (!File.Exists(filePath))
            {
                return [];
            }

            byte[] payload = File.ReadAllBytes(filePath);

            if (payload.Length <= NonceSizeBytes + TagSizeBytes)
            {
                return [];
            }

            byte[] nonce = payload[..NonceSizeBytes];
            byte[] tag = payload[NonceSizeBytes..(NonceSizeBytes + TagSizeBytes)];
            byte[] cipherText = payload[(NonceSizeBytes + TagSizeBytes)..];
            byte[] plainText = new byte[cipherText.Length];

            using AesGcm aesGcm = new(encryptionKey, TagSizeBytes);
            aesGcm.Decrypt(nonce, cipherText, tag, plainText);

            return JsonSerializer.Deserialize<SteamAccount[]>(plainText, SerializerOptions)?.ToList() ?? [];
        }

        private void PersistAccounts()
        {
            byte[] plainText = JsonSerializer.SerializeToUtf8Bytes(accounts.ToArray(), SerializerOptions);
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
    }
}
