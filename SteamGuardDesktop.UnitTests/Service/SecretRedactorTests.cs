using NUnit.Framework;

using SteamGuardDesktop.Service.Diagnostics;

namespace SteamGuardDesktop.UnitTests.Service
{
    [TestFixture]
    public sealed class SecretRedactorTests
    {
        [TestCase("shared-secret-value")]
        [TestCase("eyJhbGciOiJIUzI1NiJ9.payload.signature")]
        public void GivenASensitiveValue_WhenMaskingIt_ThenTheValueIsNotVisible(string sensitiveValue)
        {
            SecretRedactor redactor = new();

            Assert.That(redactor.Mask(sensitiveValue), Is.EqualTo("********"));
        }

        [Test]
        public void GivenNoValue_WhenMaskingIt_ThenTheMaskIsStillReturned()
        {
            SecretRedactor redactor = new();

            Assert.That(redactor.Mask(string.Empty), Is.EqualTo("********"));
        }

        [Test]
        public void GivenAConfirmationAddress_WhenSanitisingIt_ThenTheSignedQueryIsRemoved()
        {
            SecretRedactor redactor = new();

            string sanitisedAddress = redactor.SanitiseAddress(
                "https://steamcommunity.com/mobileconf/getlist?p=device&a=76561198028572203&k=signature");

            Assert.That(
                sanitisedAddress,
                Is.EqualTo("https://steamcommunity.com/mobileconf/getlist?[redacted]"));
        }

        [Test]
        public void GivenAnAddressThatIsNotAbsolute_WhenSanitisingIt_ThenTheValueIsMasked()
        {
            SecretRedactor redactor = new();

            Assert.That(redactor.SanitiseAddress("not-an-address"), Is.EqualTo("********"));
        }
    }
}