using System;

using NUnit.Framework;

using SteamGuardDesktop.Client.Steam;
using SteamGuardDesktop.Service.Authenticator;
using SteamGuardDesktop.UnitTests.Utils;

namespace SteamGuardDesktop.UnitTests.Client
{
    [TestFixture]
    public sealed class SteamSharedSecretEncoderTests
    {
        [Test]
        public void GivenASecretInSteamByteForm_WhenEncodingIt_ThenItMatchesTheBase32FormOfTheAuthenticator()
        {
            byte[] secretBytes = Convert.FromBase64String("G9oGYQuBRAdOst6uY1djlfqcSfs=");

            Assert.That(
                SteamSharedSecretEncoder.Encode(secretBytes),
                Is.EqualTo("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3"));
        }

        [Test]
        public void GivenASecretInSteamByteForm_WhenEncodingIt_ThenTheAuthenticatorAcceptsItAndGeneratesTheKnownCode()
        {
            byte[] secretBytes = Convert.FromBase64String("G9oGYQuBRAdOst6uY1djlfqcSfs=");
            OtpCodeGenerator generator = new(
                new SteamGuard.TOTP.SteamGuard(new FixedTimeStepProvider(613)),
                new FixedClock(DateTimeOffset.UnixEpoch));

            string encodedSecret = SteamSharedSecretEncoder.Encode(secretBytes);

            Assert.That(generator.IsValidSharedSecret(encodedSecret));
            Assert.That(generator.GenerateCode(encodedSecret)?.Value, Is.EqualTo("D57RM"));
        }

        [Test]
        public void GivenASecretThatPadsTheFinalGroup_WhenEncodingIt_ThenThePaddingIsOmitted()
        {
            byte[] secretBytes = Convert.FromBase64String("AQIDBAUGBwgJCgsMDQ4PEA==");

            string encodedSecret = SteamSharedSecretEncoder.Encode(secretBytes);

            Assert.That(encodedSecret, Does.Not.Contain("="));
            Assert.That(encodedSecret, Is.EqualTo("AEBAGBAFAYDQQCIKBMGA2DQPCA"));
        }

        [Test]
        public void GivenNoBytes_WhenEncodingThem_ThenNoValueIsProduced()
        {
            Assert.That(SteamSharedSecretEncoder.Encode([]), Is.Empty);
        }
    }
}
