using System;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

using SteamGuardDesktop.DataAccess.Accounts;
using SteamGuardDesktop.Domain.Account;

namespace SteamGuardDesktop.IntegrationTests.DataAccess
{
    [TestFixture]
    public sealed class EncryptedFileAccountStoreTests
    {
        private string filePath = string.Empty;

        [SetUp]
        public void SetUp() => filePath = Path.Combine(
            Path.GetTempPath(),
            "SteamGuardDesktopTests",
            Guid.NewGuid().ToString("N"),
            "accounts.bin");

        [TearDown]
        public void TearDown()
        {
            string? directory = Path.GetDirectoryName(filePath);

            if (directory is not null && Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        [Test]
        public void GivenASavedAccount_WhenTheStoreIsRecreated_ThenTheAccountIsStillAvailable()
        {
            EncryptedFileAccountStore store = new(filePath, "test-key-seed");
            store.Save(new SteamAccount { Identifier = "76561198000000000", Label = "Primary" });

            EncryptedFileAccountStore reloadedStore = new(filePath, "test-key-seed");

            Assert.That(reloadedStore.GetAll().Select(account => account.Identifier), Does.Contain("76561198000000000"));
        }

        [Test]
        public void GivenASavedAccount_WhenReadingTheFile_ThenTheAccountIsNotPresentInPlainText()
        {
            EncryptedFileAccountStore store = new(filePath, "test-key-seed");
            store.Save(new SteamAccount { Identifier = "76561198000000000", Label = "Primary" });

            string content = Encoding.UTF8.GetString(File.ReadAllBytes(filePath));

            Assert.That(content, Does.Not.Contain("76561198000000000"));
            Assert.That(content, Does.Not.Contain("Primary"));
        }

        [Test]
        public void GivenAnEmptyFile_WhenTheStoreIsCreated_ThenNoAccountIsLoaded()
        {
            string? directory = Path.GetDirectoryName(filePath);

            if (directory is not null)
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(filePath, []);

            EncryptedFileAccountStore store = new(filePath, "test-key-seed");

            Assert.That(store.GetAll(), Is.Empty);
        }

        [Test]
        public void GivenATruncatedFile_WhenTheStoreIsCreated_ThenTheFileIsRejected()
        {
            string? directory = Path.GetDirectoryName(filePath);

            if (directory is not null)
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(filePath, new byte[8]);

            Assert.Catch<System.Security.Cryptography.CryptographicException>(
                () => _ = new EncryptedFileAccountStore(filePath, "test-key-seed"));
        }

        [Test]
        public void GivenAForeignKeyFile_WhenTheStoreIsCreated_ThenTheFileIsRejected()
        {
            EncryptedFileAccountStore store = new(filePath, "test-key-seed");
            store.Save(new SteamAccount { Identifier = "76561198000000000", Label = "Primary" });

            Assert.Throws<System.Security.Cryptography.AuthenticationTagMismatchException>(
                () => _ = new EncryptedFileAccountStore(filePath, "another-key-seed"));
        }
    }
}
