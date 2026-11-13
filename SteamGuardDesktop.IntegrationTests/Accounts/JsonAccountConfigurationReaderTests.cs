using System;
using System.IO;

using NUnit.Framework;

using SteamGuardDesktop.DataAccess.Accounts;
using SteamGuardDesktop.Domain.Authenticator;

namespace SteamGuardDesktop.IntegrationTests.Accounts
{
    [TestFixture]
    public sealed class JsonAccountConfigurationReaderTests
    {
        private string filePath = string.Empty;

        [SetUp]
        public void SetUp()
        {
            string directory = Path.Combine(
                Path.GetTempPath(),
                "SteamGuardDesktopTests",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(directory);
            filePath = Path.Combine(directory, "account.json");
        }

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
        public void GivenAValidConfigurationFile_WhenReadingIt_ThenTheAccountAndSecretsAreProduced()
        {
            File.WriteAllText(filePath, """
                {"steamid":"76561198028572203","label":"DummyUser",
                 "shared_secret":"DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3",
                 "identity_secret":"GQP46b73Ws7gr8GmZFR0sDuau5c=",
                 "revocation_code":"R61387","access_token":"access-token-value"}
                """);

            JsonAccountConfigurationReader reader = new();
            AccountConfigurationReadResult result = reader.Read(
                filePath,
                out Domain.Account.SteamAccount? account,
                out AuthenticatorSecrets? secrets);

            Assert.That(result.IsSuccessful);
            Assert.That(account?.SteamIdentifier, Is.EqualTo("76561198028572203"));
            Assert.That(account?.Label, Is.EqualTo("DummyUser"));
            Assert.That(secrets?.SharedSecret, Is.EqualTo("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3"));
            Assert.That(secrets?.AccessToken, Is.EqualTo("access-token-value"));
        }

        [Test]
        public void GivenAConfigurationFileWithoutASteamIdentifier_WhenReadingIt_ThenTheRequestFails()
        {
            File.WriteAllText(filePath, """{"shared_secret":"DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3"}""");

            JsonAccountConfigurationReader reader = new();
            AccountConfigurationReadResult result = reader.Read(
                filePath,
                out Domain.Account.SteamAccount? account,
                out AuthenticatorSecrets? secrets);

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(account, Is.Null);
            Assert.That(secrets, Is.Null);
        }

        [Test]
        public void GivenAMalformedConfigurationFile_WhenReadingIt_ThenTheRequestFails()
        {
            File.WriteAllText(filePath, "{not-json");

            JsonAccountConfigurationReader reader = new();
            AccountConfigurationReadResult result = reader.Read(
                filePath,
                out Domain.Account.SteamAccount? account,
                out AuthenticatorSecrets? secrets);

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(account, Is.Null);
        }

        [Test]
        public void GivenNoConfigurationFile_WhenReadingIt_ThenTheRequestFails()
        {
            JsonAccountConfigurationReader reader = new();
            AccountConfigurationReadResult result = reader.Read(
                filePath,
                out Domain.Account.SteamAccount? account,
                out AuthenticatorSecrets? secrets);

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.FailureReason, Does.Contain("does not exist"));
        }

        [Test]
        public void GivenNoPath_WhenReadingAConfigurationFile_ThenTheRequestFails()
        {
            JsonAccountConfigurationReader reader = new();

            AccountConfigurationReadResult result = reader.Read(
                string.Empty,
                out Domain.Account.SteamAccount? account,
                out AuthenticatorSecrets? secrets);

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(account, Is.Null);
        }
    }
}
