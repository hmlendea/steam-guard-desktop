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
    public sealed class AccountServiceTests
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
                    new FixedClock(System.DateTimeOffset.UnixEpoch)));
        }

        [Test]
        public void GivenValidAccountData_WhenSavingTheAccount_ThenTheDeviceIdentifierAndSecretsAreStored()
        {
            AccountModel account = BuildAccount();

            AccountSaveResult result = accountService.SaveAccount(account, BuildSecrets());

            Assert.That(result.IsSuccessful);
            Assert.That(result.Account?.DeviceIdentifier, Is.EqualTo("186afecb275bf6db9bb855caa9c916dc56b2dc6a"));
            Assert.That(secretStore.Retrieve(result.Account?.SharedSecretName ?? string.Empty), Is.EqualTo(ValidSharedSecret));
        }

        [Test]
        public void GivenASteamIdentifierWithInvalidLength_WhenSavingTheAccount_ThenValidationFails()
        {
            AccountModel account = BuildAccount();
            account.SteamIdentifier = "613";

            AccountSaveResult result = accountService.SaveAccount(account, BuildSecrets());

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.ValidationErrors, Has.Exactly(1).Items);
        }

        [Test]
        public void GivenAnInvalidSharedSecret_WhenSavingTheAccount_ThenValidationFails()
        {
            AuthenticatorSecrets secrets = BuildSecrets();
            secrets.SharedSecret = "not-base32";

            AccountSaveResult result = accountService.SaveAccount(BuildAccount(), secrets);

            Assert.That(result.IsSuccessful, Is.False);
        }

        [Test]
        public void GivenAnInvalidIdentitySecret_WhenSavingTheAccount_ThenValidationFails()
        {
            AuthenticatorSecrets secrets = BuildSecrets();
            secrets.IdentitySecret = "not-base64";

            AccountSaveResult result = accountService.SaveAccount(BuildAccount(), secrets);

            Assert.That(result.IsSuccessful, Is.False);
        }

        [Test]
        public void GivenAnAlreadyConfiguredSteamIdentifier_WhenSavingAnotherAccount_ThenValidationFails()
        {
            accountService.SaveAccount(BuildAccount(), BuildSecrets());

            AccountModel duplicateAccount = BuildAccount();
            duplicateAccount.Identifier = "another-account";

            AccountSaveResult result = accountService.SaveAccount(duplicateAccount, BuildSecrets());

            Assert.That(result.IsSuccessful, Is.False);
        }

        [Test]
        public void GivenACredentialStoreThatCannotBeWritten_WhenSavingTheAccount_ThenTheOperationFails()
        {
            AccountService failingAccountService = new(
                accountStore,
                new InMemorySecretStore(SecretStoreAvailability.Unavailable),
                new DeviceIdentityProvider(),
                new OtpCodeGenerator(
                    new SteamGuard.TOTP.SteamGuard(new FixedTimeStepProvider(1L)),
                    new FixedClock(System.DateTimeOffset.UnixEpoch)));

            AccountSaveResult result = failingAccountService.SaveAccount(BuildAccount(), BuildSecrets());

            Assert.That(result.IsSuccessful, Is.False);
        }

        [Test]
        public void GivenAConfiguredAccount_WhenRemovingIt_ThenItsSecretsAreDeleted()
        {
            AccountSaveResult saveResult = accountService.SaveAccount(BuildAccount(), BuildSecrets());

            AccountRemovalResult removalResult = accountService.RemoveAccount(saveResult.Account?.Identifier ?? string.Empty);

            Assert.That(removalResult.IsSuccessful);
            Assert.That(accountService.GetAccounts(), Is.Empty);
            Assert.That(secretStore.Retrieve(saveResult.Account?.SharedSecretName ?? string.Empty), Is.Null);
        }

        [Test]
        public void GivenAnUnknownAccount_WhenRemovingIt_ThenTheOperationFails()
        {
            AccountRemovalResult result = accountService.RemoveAccount("unknown-account");

            Assert.That(result.IsSuccessful, Is.False);
        }

        [Test]
        public void GivenAConfiguredAccount_WhenRetrievingItsSecrets_ThenTheyAreReturned()
        {
            AccountSaveResult saveResult = accountService.SaveAccount(BuildAccount(), BuildSecrets());

            AuthenticatorSecrets? secrets = accountService.GetSecrets(saveResult.Account!);

            Assert.That(secrets, Is.Not.Null);
            Assert.That(secrets?.IdentitySecret, Is.EqualTo(ValidIdentitySecret));
        }

        [Test]
        public void GivenAnAccountWithoutStoredSecrets_WhenRetrievingItsSecrets_ThenNothingIsReturned()
        {
            AccountModel account = BuildAccount();
            account.SharedSecretName = "unknown-shared-secret";

            Assert.That(accountService.GetSecrets(account), Is.Null);
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
