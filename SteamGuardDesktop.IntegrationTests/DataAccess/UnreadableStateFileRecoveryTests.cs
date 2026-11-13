using System;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

using SteamGuardDesktop.DataAccess.Accounts;
using SteamGuardDesktop.Service.Storage;

namespace SteamGuardDesktop.IntegrationTests.DataAccess
{
    [TestFixture]
    public sealed class UnreadableStateFileRecoveryTests
    {
        private string directoryPath = string.Empty;

        [SetUp]
        public void SetUp() => directoryPath = Path.Combine(
            Path.GetTempPath(),
            "SteamGuardDesktopTests",
            Guid.NewGuid().ToString("N"));

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }
        }

        [Test]
        public void GivenAStateFileWrittenWithAnotherKey_WhenRecovered_ThenTheApplicationStateStartsEmpty()
        {
            string accountStorePath = Path.Combine(directoryPath, "accounts.bin");
            string secretStorePath = Path.Combine(directoryPath, "secrets.bin");

            Directory.CreateDirectory(directoryPath);

            EncryptedFileAccountStore originalAccountStore = new(accountStorePath, "original-key-seed");
            originalAccountStore.Save(new Domain.Account.SteamAccount { Identifier = "76561198000000000" });

            EncryptedFileSecretStore originalSecretStore = new(secretStorePath, "original-key-seed");
            originalSecretStore.Store("76561198000000000:shared", "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");

            Assert.Catch<System.Security.Cryptography.CryptographicException>(
                () => _ = new EncryptedFileAccountStore(accountStorePath, "renamed-machine-key-seed"));
            Assert.Catch<System.Security.Cryptography.CryptographicException>(
                () => _ = new EncryptedFileSecretStore(secretStorePath, "renamed-machine-key-seed"));

            UnreadableStateFileRecovery.Quarantine(accountStorePath);
            UnreadableStateFileRecovery.Quarantine(secretStorePath);

            EncryptedFileAccountStore recoveredAccountStore = new(accountStorePath, "renamed-machine-key-seed");
            EncryptedFileSecretStore recoveredSecretStore = new(secretStorePath, "renamed-machine-key-seed");

            Assert.That(recoveredAccountStore.GetAll(), Is.Empty);
            Assert.That(recoveredSecretStore.Retrieve("76561198000000000:shared"), Is.Null);
            Assert.That(
                File.Exists(UnreadableStateFileRecovery.ComposeQuarantinedPath(accountStorePath)),
                Is.True);
            Assert.That(
                File.Exists(UnreadableStateFileRecovery.ComposeQuarantinedPath(secretStorePath)),
                Is.True);
        }

        [Test]
        public void GivenACorruptedStateFile_WhenRecovered_ThenTheOriginalPayloadIsPreserved()
        {
            string accountStorePath = Path.Combine(directoryPath, "accounts.bin");

            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(accountStorePath, "corrupted-payload", Encoding.UTF8);

            string? quarantinedPath = UnreadableStateFileRecovery.Quarantine(accountStorePath);
            EncryptedFileAccountStore recoveredAccountStore = new(accountStorePath, "any-key-seed");

            Assert.That(recoveredAccountStore.GetAll(), Is.Empty);
            Assert.That(quarantinedPath, Is.Not.Null);
            Assert.That(File.Exists(quarantinedPath!), Is.True);
            Assert.That(File.ReadAllText(quarantinedPath!), Is.EqualTo("corrupted-payload"));
        }

        [Test]
        public void GivenAValidStateFile_WhenNotRecovered_ThenTheAccountsAreStillLoaded()
        {
            string accountStorePath = Path.Combine(directoryPath, "accounts.bin");

            Directory.CreateDirectory(directoryPath);

            EncryptedFileAccountStore store = new(accountStorePath, "stable-key-seed");
            store.Save(new Domain.Account.SteamAccount { Identifier = "76561198000000000" });

            EncryptedFileAccountStore reloadedStore = new(accountStorePath, "stable-key-seed");

            Assert.That(reloadedStore.GetAll().Select(account => account.Identifier), Does.Contain("76561198000000000"));
            Assert.That(
                File.Exists(UnreadableStateFileRecovery.ComposeQuarantinedPath(accountStorePath)),
                Is.False);
        }
    }
}
