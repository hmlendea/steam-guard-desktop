using System;

using NUnit.Framework;

using SteamGuard.TOTP;
using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Service.Authenticator;
using SteamGuardDesktop.UnitTests.Utils;

using TOTPSteamGuard = SteamGuard.TOTP.SteamGuard;

namespace SteamGuardDesktop.UnitTests.Service
{
    [TestFixture]
    public sealed class OtpCodeGeneratorTests
    {
        private static readonly DateTimeOffset FixedMoment = DateTimeOffset.FromUnixTimeSeconds(1617591917);

        [TestCase("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3", 613, "D57RM")]
        [TestCase("SQUTXBT56XIDXZ63UBVVMK3ORG4ZDRCF", 873, "W28WD")]
        public void GivenAKnownSharedSecret_WhenGeneratingACode_ThenTheKnownCodeIsProduced(
            string sharedSecret,
            long timeStep,
            string expectedCode)
        {
            OtpCodeGenerator generator = BuildGenerator(timeStep);

            Assert.That(generator.GenerateCode(sharedSecret)?.Value, Is.EqualTo(expectedCode));
        }

        [TestCase("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3")]
        [TestCase("SQUTXBT56XIDXZ63UBVVMK3ORG4ZDRCF")]
        public void GivenAValidSharedSecret_WhenValidatingIt_ThenItIsAccepted(string sharedSecret)
        {
            OtpCodeGenerator generator = BuildGenerator(1L);

            Assert.That(generator.IsValidSharedSecret(sharedSecret));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("NOT-BASE32-!!")]
        [TestCase("0189")]
        public void GivenAnInvalidSharedSecret_WhenValidatingIt_ThenItIsRejected(string sharedSecret)
        {
            OtpCodeGenerator generator = BuildGenerator(1L);

            Assert.That(generator.IsValidSharedSecret(sharedSecret), Is.False);
        }

        [Test]
        public void GivenAnInvalidSharedSecret_WhenGeneratingACode_ThenNoCodeIsProduced()
        {
            OtpCodeGenerator generator = BuildGenerator(1L);

            Assert.That(generator.GenerateCode("invalid secret"), Is.Null);
        }

        [Test]
        public void GivenAFixedClock_WhenGeneratingACode_ThenTheExpiryIsTheNextThirtySecondBoundary()
        {
            OtpCodeGenerator generator = BuildGenerator(1L);

            SteamGuardCode? code = generator.GenerateCode("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");

            Assert.That(code?.ExpiresAt, Is.EqualTo(FixedMoment.AddSeconds(13)));
        }

        [Test]
        public void GivenAGeneratedCode_WhenEvaluatingItAfterItsExpiry_ThenItIsInvalid()
        {
            OtpCodeGenerator generator = BuildGenerator(1L);

            SteamGuardCode? code = generator.GenerateCode("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");

            Assert.That(code?.IsValidAt(FixedMoment.AddSeconds(20)), Is.False);
        }

        [Test]
        public void GivenAGeneratedCode_WhenEvaluatingItBeforeItsExpiry_ThenItIsValid()
        {
            OtpCodeGenerator generator = BuildGenerator(1L);

            SteamGuardCode? code = generator.GenerateCode("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");

            Assert.That(code?.IsValidAt(FixedMoment.AddSeconds(1)), Is.True);
        }

        private static OtpCodeGenerator BuildGenerator(long timeStep)
        {
            ISteamGuard steamGuard = new TOTPSteamGuard(new FixedTimeStepProvider(timeStep));

            return new OtpCodeGenerator(steamGuard, new FixedClock(FixedMoment));
        }
    }
}
