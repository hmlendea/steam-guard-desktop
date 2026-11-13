using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using NUnit.Framework;

using SteamGuardDesktop.Service.Storage;

namespace SteamGuardDesktop.IntegrationTests.DataAccess
{
    [TestFixture]
    public sealed class EncryptedFileSecretStoreTests
    {
        private string filePath = string.Empty;

        [SetUp]
        public void SetUp() => filePath = Path.Combine(
            Path.GetTempPath(),
            "SteamGuardDesktopTests",
            Guid.NewGuid().ToString("N"),
            "secrets.bin");

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
        public void GivenAStoredSecret_WhenReadingTheFile_ThenTheSecretIsNotPresentInPlainText()
        {
            EncryptedFileSecretStore store = new(filePath, "test-key-seed");
            store.Store("account:shared", "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");

            byte[] payload = File.ReadAllBytes(filePath);
            string content = Encoding.UTF8.GetString(payload);

            Assert.That(content, Does.Not.Contain("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3"));
        }

        [Test]
        public void GivenAStoredSecret_WhenRetrievingIt_ThenItIsReturned()
        {
            EncryptedFileSecretStore store = new(filePath, "test-key-seed");
            store.Store("account:shared", "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");

            Assert.That(store.Retrieve("account:shared"), Is.EqualTo("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3"));
        }

        [Test]
        public void GivenAStoredSecret_WhenCreatingAnotherStoreOverTheSameFile_ThenTheSecretIsStillReadable()
        {
            EncryptedFileSecretStore store = new(filePath, "test-key-seed");
            store.Store("account:shared", "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");

            EncryptedFileSecretStore reloadedStore = new(filePath, "test-key-seed");

            Assert.That(reloadedStore.Retrieve("account:shared"), Is.EqualTo("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3"));
        }

        [Test]
        public void GivenAStoredSecret_WhenRemovingIt_ThenItCanNoLongerBeRead()
        {
            EncryptedFileSecretStore store = new(filePath, "test-key-seed");
            store.Store("account:shared", "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");
            store.Remove("account:shared");

            Assert.That(store.Retrieve("account:shared"), Is.Null);
        }

        [Test]
        public void GivenAStoredSecret_WhenTheFileIsReadWithAnotherKey_ThenTheSecretCannotBeRead()
        {
            EncryptedFileSecretStore store = new(filePath, "test-key-seed");
            store.Store("account:shared", "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");

            Assert.Throws<AuthenticationTagMismatchException>(() => _ = new EncryptedFileSecretStore(filePath, "another-key-seed"));
        }
    }
}
