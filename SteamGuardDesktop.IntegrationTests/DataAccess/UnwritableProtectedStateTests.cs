using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using SteamGuardDesktop.DataAccess.Accounts;
using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Service.Configuration;
using SteamGuardDesktop.Service.Storage;

namespace SteamGuardDesktop.IntegrationTests.DataAccess
{
    [TestFixture]
    public sealed class UnwritableProtectedStateTests
    {
        private string directoryPath = string.Empty;

        [SetUp]
        public void SetUp()
        {
            directoryPath = Path.Combine(
                Path.GetTempPath(),
                "SteamGuardDesktopTests",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(directoryPath);
        }

        [TearDown]
        public void TearDown()
        {
            MakeWritable(directoryPath);

            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }
        }

        [Test]
        public void GivenAReadOnlyDirectory_WhenStoringASecret_ThenItIsReportedAsFailure()
        {
            Assume.That(CanEnforceReadOnlyDirectory(directoryPath), "The host does not enforce the permissions.");

            MakeReadOnly(directoryPath);

            EncryptedFileSecretStore store = new(
                Path.Combine(directoryPath, "secrets.bin"),
                "test-machine:test-user");

            SecretStoreResult result = store.Store(
                "76561198000000000:shared",
                "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.FailureReason, Is.Not.Empty);
        }

        [Test]
        public void GivenAReadOnlyDirectory_WhenStoringASecret_ThenNothingIsKeptInMemory()
        {
            Assume.That(CanEnforceReadOnlyDirectory(directoryPath), "The host does not enforce the permissions.");

            MakeReadOnly(directoryPath);

            EncryptedFileSecretStore store = new(
                Path.Combine(directoryPath, "secrets.bin"),
                "test-machine:test-user");

            store.Store("76561198000000000:shared", "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");

            Assert.That(store.Retrieve("76561198000000000:shared"), Is.Null);
        }

        [Test]
        public void GivenAReadOnlyExistingFile_WhenStoringASecret_ThenThePreviousValueIsRestored()
        {
            Assume.That(CanEnforceReadOnlyFile(directoryPath), "The host does not enforce the permissions.");

            string secretStorePath = Path.Combine(directoryPath, "secrets.bin");
            EncryptedFileSecretStore store = new(secretStorePath, "test-machine:test-user");

            store.Store("76561198000000000:shared", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            MakeFileReadOnly(secretStorePath);

            SecretStoreResult result = store.Store("76561198000000000:shared", "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(store.Retrieve("76561198000000000:shared"), Is.EqualTo("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
        }

        [Test]
        public void GivenAReadOnlyExistingFile_WhenRemovingASecret_ThenTheEntryIsRestored()
        {
            Assume.That(CanEnforceReadOnlyFile(directoryPath), "The host does not enforce the permissions.");

            string secretStorePath = Path.Combine(directoryPath, "secrets.bin");
            EncryptedFileSecretStore store = new(secretStorePath, "test-machine:test-user");

            store.Store("76561198000000000:shared", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            MakeFileReadOnly(secretStorePath);

            SecretStoreResult result = store.Remove("76561198000000000:shared");

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(store.Retrieve("76561198000000000:shared"), Is.EqualTo("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
        }

        [Test]
        public void GivenAReadOnlyDirectory_WhenSavingAnAccount_ThenAnIoExceptionIsRaised()
        {
            Assume.That(CanEnforceReadOnlyDirectory(directoryPath), "The host does not enforce the permissions.");

            MakeReadOnly(directoryPath);

            EncryptedFileAccountStore accountStore = new(
                Path.Combine(directoryPath, "accounts.bin"),
                "test-machine:test-user");

            Assert.Throws<IOException>(
                () => accountStore.Save(new SteamAccount { Identifier = "76561198000000000" }));
        }

        [Test]
        public void GivenAReadOnlyDirectory_WhenSavingAnAccount_ThenItIsNotKeptInMemory()
        {
            Assume.That(CanEnforceReadOnlyDirectory(directoryPath), "The host does not enforce the permissions.");

            MakeReadOnly(directoryPath);

            EncryptedFileAccountStore accountStore = new(
                Path.Combine(directoryPath, "accounts.bin"),
                "test-machine:test-user");

            try
            {
                accountStore.Save(new SteamAccount { Identifier = "76561198000000000" });
            }
            catch (IOException)
            {
            }

            Assert.That(accountStore.GetAll(), Is.Empty);
        }

        [Test]
        public void GivenAReadOnlyDirectory_WhenSavingTheSettings_ThenTheFailureIsReported()
        {
            Assume.That(CanEnforceReadOnlyDirectory(directoryPath), "The host does not enforce the permissions.");

            MakeReadOnly(directoryPath);

            JsonFileSettingsStore settingsStore = new(Path.Combine(directoryPath, "settings.json"));

            bool wasSaved = settingsStore.Save(new SettingsSnapshot { IsSecretRevealAllowed = true });

            Assert.That(wasSaved, Is.False);
        }

        [Test]
        public void GivenAWritableDirectory_WhenSavingTheSettings_ThenTheyAreStored()
        {
            JsonFileSettingsStore settingsStore = new(Path.Combine(directoryPath, "settings.json"));

            bool wasSaved = settingsStore.Save(new SettingsSnapshot { IsSecretRevealAllowed = true });

            Assert.That(wasSaved, Is.True);
            Assert.That(settingsStore.Load().IsSecretRevealAllowed, Is.True);
        }

        [Test]
        public void GivenAWritableDirectory_WhenStoringASecret_ThenItIsReportedAsSuccess()
        {
            EncryptedFileSecretStore store = new(
                Path.Combine(directoryPath, "secrets.bin"),
                "test-machine:test-user");

            SecretStoreResult result = store.Store(
                "76561198000000000:shared",
                "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");

            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(store.Retrieve("76561198000000000:shared"), Is.EqualTo("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3"));
        }

        [Test]
        public void GivenAWritableDirectory_WhenSavingMultipleAccounts_ThenEveryAccountIsKept()
        {
            EncryptedFileAccountStore accountStore = new(
                Path.Combine(directoryPath, "accounts.bin"),
                "test-machine:test-user");

            accountStore.Save(new SteamAccount { Identifier = "76561198000000000" });
            accountStore.Save(new SteamAccount { Identifier = "76561198000000001" });

            List<SteamAccount> accounts = [.. accountStore.GetAll()];

            Assert.That(accounts, Has.Count.EqualTo(2));
        }

        [Test]
        public void GivenAWritableDirectory_WhenRemovingASecret_ThenItIsReportedAsSuccess()
        {
            EncryptedFileSecretStore store = new(
                Path.Combine(directoryPath, "secrets.bin"),
                "test-machine:test-user");

            store.Store("76561198000000000:shared", "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");

            SecretStoreResult result = store.Remove("76561198000000000:shared");

            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(store.Retrieve("76561198000000000:shared"), Is.Null);
        }

        private static bool CanEnforceReadOnlyDirectory(string path)
        {
            if (OperatingSystem.IsWindows())
            {
                return false;
            }

            MakeReadOnly(path);
            bool isEnforced = !CanCreateFile(path);
            MakeWritable(path);

            return isEnforced;
        }

        private static bool CanEnforceReadOnlyFile(string path)
        {
            if (OperatingSystem.IsWindows())
            {
                return false;
            }

            string probePath = Path.Combine(path, "probe.bin");

            File.WriteAllText(probePath, "probe");
            MakeFileReadOnly(probePath);
            bool isEnforced = !CanWriteToFile(probePath);

            MakeFileWritable(probePath);
            File.Delete(probePath);

            return isEnforced;
        }

        private static bool CanCreateFile(string path)
        {
            string probePath = Path.Combine(path, "probe.bin");

            try
            {
                File.WriteAllText(probePath, "probe");

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            finally
            {
                if (File.Exists(probePath))
                {
                    File.Delete(probePath);
                }
            }
        }

        private static bool CanWriteToFile(string filePath)
        {
            try
            {
                File.WriteAllText(filePath, "changed");

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static void MakeReadOnly(string path)
        {
            if (OperatingSystem.IsWindows())
            {
                return;
            }

            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserExecute);
        }

        private static void MakeWritable(string path)
        {
            if (OperatingSystem.IsWindows())
            {
                return;
            }

            File.SetUnixFileMode(
                path,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        private static void MakeFileReadOnly(string filePath)
        {
            if (OperatingSystem.IsWindows())
            {
                return;
            }

            File.SetUnixFileMode(filePath, UnixFileMode.UserRead);
        }

        private static void MakeFileWritable(string filePath)
        {
            if (OperatingSystem.IsWindows())
            {
                return;
            }

            File.SetUnixFileMode(filePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }
}
