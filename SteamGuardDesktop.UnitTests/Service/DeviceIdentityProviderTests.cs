using NUnit.Framework;

using SteamGuardDesktop.Service.Authenticator;

namespace SteamGuardDesktop.UnitTests.Service
{
    [TestFixture]
    public sealed class DeviceIdentityProviderTests
    {
        [TestCase("76561198028572203", "186afecb275bf6db9bb855caa9c916dc56b2dc6a")]
        [TestCase("76561197960287930", "2d7c26a1d0163311892abc9a235e32f9314df4e4")]
        public void GivenASteamIdentifier_WhenDerivingTheDeviceIdentifier_ThenTheKnownValueIsProduced(
            string steamIdentifier,
            string expectedIdentifier)
        {
            DeviceIdentityProvider provider = new();

            Assert.That(provider.DeriveDeviceIdentifier(steamIdentifier), Is.EqualTo(expectedIdentifier));
        }

        [Test]
        public void GivenNoSteamIdentifier_WhenDerivingTheDeviceIdentifier_ThenAnExceptionIsThrown()
        {
            DeviceIdentityProvider provider = new();

            Assert.Throws<System.ArgumentException>(() => provider.DeriveDeviceIdentifier(string.Empty));
        }
    }
}