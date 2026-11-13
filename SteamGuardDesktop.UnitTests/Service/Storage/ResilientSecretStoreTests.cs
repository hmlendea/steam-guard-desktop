using NUnit.Framework;

using SteamGuardDesktop.Service.Storage;
using SteamGuardDesktop.UnitTests.Utils;

namespace SteamGuardDesktop.UnitTests.Service.Storage
{
    [TestFixture]
    public sealed class ResilientSecretStoreTests
    {
        private const string EntryName = "76561198028572203:shared";

        private const string SecretValue = "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3";

        [Test]
        public void GivenAnUnreachableNativeStore_WhenCheckingAvailability_ThenItIsReportedUnavailable()
        {
            ResilientSecretStore store = new(
                new InMemoryNativeSecretBackend(SecretStoreAvailability.Unavailable),
                new InMemorySecretStore());

            Assert.That(store.GetAvailability(), Is.EqualTo(SecretStoreAvailability.Unavailable));
        }

        [Test]
        public void GivenAnAvailableNativeStore_WhenCheckingAvailability_ThenItIsReportedAvailable()
        {
            ResilientSecretStore store = new(
                new InMemoryNativeSecretBackend(),
                new InMemorySecretStore(SecretStoreAvailability.Unavailable));

            Assert.That(store.GetAvailability(), Is.EqualTo(SecretStoreAvailability.Available));
        }

        [Test]
        public void GivenAnAvailableNativeStore_WhenStoringASecret_ThenTheSecretIsStoredNatively()
        {
            InMemoryNativeSecretBackend nativeBackend = new();
            InMemorySecretStore fallbackStore = new();
            ResilientSecretStore store = new(nativeBackend, fallbackStore);

            SecretStoreResult result = store.Store(EntryName, SecretValue);

            Assert.That(result.IsSuccessful);
            Assert.That(nativeBackend.Read(EntryName), Is.EqualTo(SecretValue));
            Assert.That(fallbackStore.Retrieve(EntryName), Is.Null);
        }

        [Test]
        public void GivenAnUnreachableNativeStore_WhenStoringASecret_ThenTheFallbackKeepsTheSecret()
        {
            InMemoryNativeSecretBackend nativeBackend = new(SecretStoreAvailability.Unavailable);
            InMemorySecretStore fallbackStore = new();
            ResilientSecretStore store = new(nativeBackend, fallbackStore);

            SecretStoreResult result = store.Store(EntryName, SecretValue);

            Assert.That(result.IsSuccessful);
            Assert.That(nativeBackend.Read(EntryName), Is.Null);
            Assert.That(fallbackStore.Retrieve(EntryName), Is.EqualTo(SecretValue));
        }

        [Test]
        public void GivenASecretInTheNativeStore_WhenRetrievingIt_ThenTheNativeValueIsReturned()
        {
            InMemoryNativeSecretBackend nativeBackend = new();
            nativeBackend.Write(EntryName, SecretValue);

            InMemorySecretStore fallbackStore = new();
            fallbackStore.Store(EntryName, "GQP46b73Ws7gr8GmZFR0sDuau5c=");

            ResilientSecretStore store = new(nativeBackend, fallbackStore);

            Assert.That(store.Retrieve(EntryName), Is.EqualTo(SecretValue));
        }
    }
}
