using NUnit.Framework;

using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Service.Account;
using SteamGuardDesktop.Service.Authenticator;
using SteamGuardDesktop.Service.Storage;
using SteamGuardDesktop.UnitTests.Utils;

using AccountModel = SteamGuardDesktop.Domain.Account.SteamAccount;

namespace SteamGuardDesktop.UnitTests.Service
{
    [TestFixture]
    public sealed class AccountAccessTokenTests
    {
        private const string ValidSteamIdentifier = "76561198028572203";

        private const string ValidSharedSecret = "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3";

        private const string ValidIdentitySecret = "GQP46b73Ws7gr8GmZFR0sDuau5c=";

        private InMemoryAccountStore accountStore = null!;

        private InMemorySecretStore secretStore = null!;

        private AccountService accountService = null!;

        [SetUp]
        public void SetUp()
        {
            accountStore = new InMemoryAccountStore();
            secretStore = new InMemorySecretStore();
            accountService = new AccountService(
                accountStore,
                secretStore,
                new DeviceIdentityProvider(),
                new OtpCodeGenerator(
                    new SteamGuard.TOTP.SteamGuard(new FixedTimeStepProvider(1L)),
                    new FixedClock(System.DateTimeOffset.UnixEpoch)),
                new NuciLog.Core.NullLogger());
        }

        [Test]
        public void GivenAConfiguredAccount_WhenSavingAnAccessToken_ThenItCanBeRetrieved()
        {
            AccountSaveResult saveResult = accountService.SaveAccount(BuildAccount(), BuildSecrets());

            AccountAccessTokenResult tokenResult = accountService.SaveAccessToken(ValidSteamIdentifier, "access-token-value");

            Assert.That(tokenResult.IsSuccessful);
            Assert.That(
                accountService.GetSecrets(saveResult.Account!)?.AccessToken,
                Is.EqualTo("access-token-value"));
        }

        [Test]
        public void GivenAConfiguredAccountWithoutAnAccessTokenName_WhenSavingAnAccessToken_ThenTheNameIsAssigned()
        {
            AccountModel account = BuildAccount();
            account.SharedSecretName = "shared";
            account.IdentitySecretName = "identity";
            accountStore.Save(account);
            secretStore.Store("shared", ValidSharedSecret);
            secretStore.Store("identity", ValidIdentitySecret);

            AccountAccessTokenResult tokenResult = accountService.SaveAccessToken(ValidSteamIdentifier, "access-token-value");

            Assert.That(tokenResult.IsSuccessful);
            Assert.That(account.AccessTokenName, Is.Not.Empty);
        }

        [Test]
        public void GivenAnUnknownAccount_WhenSavingAnAccessToken_ThenTheOperationFails()
        {
            AccountAccessTokenResult result = accountService.SaveAccessToken("76561197960287930", "access-token-value");

            Assert.That(result.IsSuccessful, Is.False);
        }

        [TestCase("")]
        [TestCase("   ")]
        public void GivenNoAccessToken_WhenSavingIt_ThenTheRequestIsRejected(string accessToken)
        {
            accountService.SaveAccount(BuildAccount(), BuildSecrets());

            Assert.Throws<System.ArgumentException>(
                () => accountService.SaveAccessToken(ValidSteamIdentifier, accessToken));
        }

        [Test]
        public void GivenNoSteamIdentifier_WhenSavingAnAccessToken_ThenTheRequestIsRejected()
        {
            Assert.Throws<System.ArgumentException>(
                () => accountService.SaveAccessToken(string.Empty, "access-token-value"));
        }

        [Test]
        public void GivenAnAccountWithAnAccessToken_WhenRemovingIt_ThenTheTokenIsDeleted()
        {
            AccountSaveResult saveResult = accountService.SaveAccount(BuildAccount(), BuildSecrets());
            accountService.SaveAccessToken(ValidSteamIdentifier, "access-token-value");

            accountService.RemoveAccount(saveResult.Account?.Identifier ?? string.Empty);

            Assert.That(secretStore.Retrieve(saveResult.Account?.AccessTokenName ?? string.Empty), Is.Null);
        }

        [Test]
        public void GivenAConfiguredAccount_WhenSavingItWithAnAccessToken_ThenTheTokenIsStored()
        {
            AccountSaveResult saveResult = accountService.SaveAccount(
                BuildAccount(),
                new AuthenticatorSecrets
                {
                    SharedSecret = ValidSharedSecret,
                    IdentitySecret = ValidIdentitySecret,
                    AccessToken = "access-token-value"
                });

            Assert.That(saveResult.IsSuccessful);
            Assert.That(
                accountService.GetSecrets(saveResult.Account!)?.AccessToken,
                Is.EqualTo("access-token-value"));
        }

        private static AccountModel BuildAccount() => new()
        {
            SteamIdentifier = ValidSteamIdentifier,
            Label = "Primary account"
        };

        private static AuthenticatorSecrets BuildSecrets() => new()
        {
            SharedSecret = ValidSharedSecret,
            IdentitySecret = ValidIdentitySecret
        };
    }
}
