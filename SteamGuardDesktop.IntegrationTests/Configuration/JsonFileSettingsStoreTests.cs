using System;
using System.IO;

using NUnit.Framework;

using SteamGuardDesktop.Service.Configuration;

namespace SteamGuardDesktop.IntegrationTests.Settings
{
    [TestFixture]
    public sealed class JsonFileSettingsStoreTests
    {
        private string filePath = string.Empty;

        [SetUp]
        public void SetUp() => filePath = Path.Combine(
            Path.GetTempPath(),
            "SteamGuardDesktopTests",
            Guid.NewGuid().ToString("N"),
            "settings.json");

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
        public void GivenNoSettingsFile_WhenLoading_ThenTheDefaultsAreReturned()
        {
            JsonFileSettingsStore store = new(filePath);

            SettingsSnapshot snapshot = store.Load();

            Assert.That(snapshot.IsAccountHiddenWhenInactive);
            Assert.That(snapshot.IsSecretRevealAllowed, Is.False);
        }

        [Test]
        public void GivenSavedSettings_WhenLoadingThem_ThenTheStoredValuesAreReturned()
        {
            JsonFileSettingsStore store = new(filePath);

            store.Save(new SettingsSnapshot
            {
                IsAccountHiddenWhenInactive = false,
                IsSecretRevealAllowed = true
            });

            SettingsSnapshot snapshot = new JsonFileSettingsStore(filePath).Load();

            Assert.That(snapshot.IsAccountHiddenWhenInactive, Is.False);
            Assert.That(snapshot.IsSecretRevealAllowed);
        }

        [Test]
        public void GivenACorruptedSettingsFile_WhenLoading_ThenTheDefaultsAreReturned()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, "{not-json");

            JsonFileSettingsStore store = new(filePath);

            Assert.That(store.Load().IsAccountHiddenWhenInactive);
        }

        [Test]
        public void GivenSettings_WhenSavingThem_ThenTheFileContainsNoSecrets()
        {
            JsonFileSettingsStore store = new(filePath);
            store.Save(new SettingsSnapshot { IsSecretRevealAllowed = true });

            string content = File.ReadAllText(filePath);

            Assert.That(content, Does.Not.Contain("secret"));
            Assert.That(content, Does.Not.Contain("token"));
        }
    }
}
