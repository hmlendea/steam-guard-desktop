using System;
using System.IO;
using System.Text;

using NUnit.Framework;

using SteamGuardDesktop.DataAccess.Accounts;
using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Service.Storage;

namespace SteamGuardDesktop.IntegrationTests.DataAccess
{
    [TestFixture]
    public sealed class ProtectedStateStoreFactoryTests
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
        public void GivenNoAccountFile_WhenCreatingTheStore_ThenNothingIsReportedAsRecovered()
        {
            IAccountStore accountStore = ProtectedStateStoreFactory.CreateAccountStore(
                ComposePath("accounts.bin"),
                "test-machine:test-user",
                out bool wasRecovered);

            Assert.That(wasRecovered, Is.False);
            Assert.That(accountStore.GetAll(), Is.Empty);
        }

        [Test]
        public void GivenAValidAccountFile_WhenCreatingTheStore_ThenTheAccountsAreLoaded()
        {
            string accountStorePath = ComposePath("accounts.bin");

            Directory.CreateDirectory(directoryPath);
            new EncryptedFileAccountStore(accountStorePath, "test-machine:test-user")
                .Save(new SteamAccount { Identifier = "76561198000000000" });

            IAccountStore accountStore = ProtectedStateStoreFactory.CreateAccountStore(
                accountStorePath,
                "test-machine:test-user",
                out bool wasRecovered);

            Assert.That(wasRecovered, Is.False);
            Assert.That(accountStore.Contains("76561198000000000"), Is.True);
        }

        [Test]
        public void GivenACorruptedAccountFile_WhenCreatingTheStore_ThenRecoveryIsReportedAndStateIsEmpty()
        {
            string accountStorePath = ComposePath("accounts.bin");

            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(accountStorePath, "not-an-encrypted-payload", Encoding.UTF8);

            IAccountStore accountStore = ProtectedStateStoreFactory.CreateAccountStore(
                accountStorePath,
                "test-machine:test-user",
                out bool wasRecovered);

            Assert.That(wasRecovered, Is.True);
            Assert.That(accountStore.GetAll(), Is.Empty);
            Assert.That(File.Exists(UnreadableStateFileRecovery.ComposeQuarantinedPath(accountStorePath)), Is.True);
        }

        [Test]
        public void GivenAnAccountFileWrittenWithAnotherKey_WhenCreatingTheStore_ThenRecoveryIsReported()
        {
            string accountStorePath = ComposePath("accounts.bin");

            Directory.CreateDirectory(directoryPath);
            new EncryptedFileAccountStore(accountStorePath, "original-machine:original-user")
                .Save(new SteamAccount { Identifier = "76561198000000000" });

            IAccountStore accountStore = ProtectedStateStoreFactory.CreateAccountStore(
                accountStorePath,
                "renamed-machine:renamed-user",
                out bool wasRecovered);

            Assert.That(wasRecovered, Is.True);
            Assert.That(accountStore.GetAll(), Is.Empty);
        }

        [Test]
        public void GivenAValidSecretFile_WhenCreatingTheStore_ThenTheSecretIsReadable()
        {
            string secretStorePath = ComposePath("secrets.bin");

            Directory.CreateDirectory(directoryPath);
            new EncryptedFileSecretStore(secretStorePath, "test-machine:test-user")
                .Store("76561198000000000:shared", "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");

            ISecretStore secretStore = ProtectedStateStoreFactory.CreateSecretStore(
                new UnavailableSecretBackend(),
                secretStorePath,
                "test-machine:test-user",
                out bool wasRecovered);

            Assert.That(wasRecovered, Is.False);
            Assert.That(secretStore.Retrieve("76561198000000000:shared"), Is.EqualTo("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3"));
        }

        [Test]
        public void GivenACorruptedSecretFile_WhenCreatingTheStore_ThenRecoveryIsReportedAndNoSecretIsReadable()
        {
            string secretStorePath = ComposePath("secrets.bin");

            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(secretStorePath, "not-an-encrypted-payload", Encoding.UTF8);

            ISecretStore secretStore = ProtectedStateStoreFactory.CreateSecretStore(
                new UnavailableSecretBackend(),
                secretStorePath,
                "test-machine:test-user",
                out bool wasRecovered);

            Assert.That(wasRecovered, Is.True);
            Assert.That(secretStore.Retrieve("76561198000000000:shared"), Is.Null);
            Assert.That(File.Exists(UnreadableStateFileRecovery.ComposeQuarantinedPath(secretStorePath)), Is.True);
        }

        private string ComposePath(string fileName) => Path.Combine(directoryPath, fileName);
    }
}
