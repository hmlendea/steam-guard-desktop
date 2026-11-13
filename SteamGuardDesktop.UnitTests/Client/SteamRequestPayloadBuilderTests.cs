using NUnit.Framework;

using SteamGuardDesktop.Client.Steam;

namespace SteamGuardDesktop.UnitTests.Client
{
    [TestFixture]
    public sealed class SteamRequestPayloadBuilderTests
    {
        [Test]
        public void GivenAnAccountName_WhenBuildingTheKeyPayload_ThenTheAccountNameIsIncluded()
        {
            SteamRequestPayloadBuilder builder = new();

            string payload = builder.BuildPasswordRsaKeyPayload("DummyUser");

            Assert.That(payload, Does.Contain("\"account_name\":\"DummyUser\""));
        }

        [Test]
        public void GivenCredentials_WhenBuildingTheSessionPayload_ThenEveryRequiredFieldIsIncluded()
        {
            SteamRequestPayloadBuilder builder = new();

            string payload = builder.BuildBeginCredentialSessionPayload(
                "DummyUser",
                "encrypted-password",
                1617591917,
                "Steam Guard Desktop");

            Assert.That(payload, Does.Contain("\"account_name\":\"DummyUser\""));
            Assert.That(payload, Does.Contain("\"encrypted_password\":\"encrypted-password\""));
            Assert.That(payload, Does.Contain("\"encryption_timestamp\":1617591917"));
            Assert.That(payload, Does.Contain("\"platform_type\":3"));
        }

        [Test]
        public void GivenAGuardCode_WhenBuildingThePayload_ThenTheGuardTypeIsIncluded()
        {
            SteamRequestPayloadBuilder builder = new();

            string payload = builder.BuildGuardCodePayload("613", "76561198028572203", "ABC12", 2);

            Assert.That(payload, Does.Contain("\"client_id\":613"));
            Assert.That(payload, Does.Contain("\"steamid\":76561198028572203"));
            Assert.That(payload, Does.Contain("\"code\":\"ABC12\""));
            Assert.That(payload, Does.Contain("\"code_type\":2"));
        }

        [Test]
        public void GivenAChallenge_WhenBuildingThePollPayload_ThenBothIdentifiersAreIncluded()
        {
            SteamRequestPayloadBuilder builder = new();

            string payload = builder.BuildPollSessionPayload("613", "873");

            Assert.That(payload, Does.Contain("\"client_id\":613"));
            Assert.That(payload, Does.Contain("\"request_id\":\"873\""));
        }

        [TestCase("")]
        [TestCase("   ")]
        public void GivenNoAccountName_WhenBuildingTheKeyPayload_ThenTheRequestIsRejected(string accountName)
        {
            SteamRequestPayloadBuilder builder = new();

            Assert.Throws<System.ArgumentException>(() => builder.BuildPasswordRsaKeyPayload(accountName));
        }

        [Test]
        public void GivenNoChallenge_WhenBuildingThePollPayload_ThenTheRequestIsRejected()
        {
            SteamRequestPayloadBuilder builder = new();

            Assert.Throws<System.ArgumentException>(() => builder.BuildPollSessionPayload(string.Empty, "873"));
        }
    }
}
