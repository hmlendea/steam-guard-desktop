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
    public sealed class AccountServiceImportTests
    {
        private const string ValidSteamIdentifier = "76561198028572203";

        private const string ValidSharedSecret = "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3";

        private const string ValidIdentitySecret = "GQP46b73Ws7gr8GmZFR0sDuau5c=";

        private InMemoryAccountStore accountStore = null!;

        private InMemorySecretStore secretStore = null!;

        private StubConfigurationReader configurationReader = null!;

        private AccountService accountService = null!;

        [SetUp]
        public void SetUp()
        {
            accountStore = new InMemoryAccountStore();
            secretStore = new InMemorySecretStore();
            configurationReader = new StubConfigurationReader();
            accountService = new AccountService(
                accountStore,
                secretStore,
                new DeviceIdentityProvider(),
                new OtpCodeGenerator(
                    new SteamGuard.TOTP.SteamGuard(new FixedTimeStepProvider(1L)),
                    new FixedClock(System.DateTimeOffset.UnixEpoch)),
                configurationReader,
                new NuciLog.Core.NullLogger());
        }

        [Test]
        public void GivenAValidConfigurationFile_WhenImportingIt_ThenTheAccountAndSecretsAreStored()
        {
            configurationReader.Account = BuildAccount();
            configurationReader.Secrets = BuildSecrets();

            AccountImportResult result = accountService.ImportAccount("account.json");

            Assert.That(result.IsSuccessful);
            Assert.That(accountStore.GetAll(), Has.Exactly(1).Items);
            Assert.That(secretStore.Retrieve(result.Account?.SharedSecretName ?? string.Empty), Is.EqualTo(ValidSharedSecret));
        }

        [Test]
        public void GivenAValidConfigurationFile_WhenImportingIt_ThenTheFileIsRemoved()
        {
            configurationReader.Account = BuildAccount();
            configurationReader.Secrets = BuildSecrets();

            AccountImportResult result = accountService.ImportAccount("account.json");

            Assert.That(result.IsConfigurationFileRemaining, Is.False);
            Assert.That(configurationReader.WasRemoved);
        }

        [Test]
        public void GivenAConfigurationFileThatCannotBeRead_WhenImportingIt_ThenTheReasonIsReported()
        {
            configurationReader.FailureReason = "The configuration file is not valid JSON.";

            AccountImportResult result = accountService.ImportAccount("account.json");

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.FailureReason, Does.Contain("not valid JSON"));
        }

        [Test]
        public void GivenAnInvalidConfigurationFile_WhenImportingIt_ThenNothingIsStored()
        {
            configurationReader.Account = BuildAccount();
            configurationReader.Secrets = new AuthenticatorSecrets
            {
                SharedSecret = "not-base32",
                IdentitySecret = ValidIdentitySecret
            };

            AccountImportResult result = accountService.ImportAccount("account.json");

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(accountStore.GetAll(), Is.Empty);
        }

        [Test]
        public void GivenAConfigurationFileThatCannotBeDeleted_WhenImportingIt_ThenTheImportStillSucceeds()
        {
            configurationReader.Account = BuildAccount();
            configurationReader.Secrets = BuildSecrets();
            configurationReader.CanBeRemoved = false;

            AccountImportResult result = accountService.ImportAccount("account.json");

            Assert.That(result.IsSuccessful);
            Assert.That(result.IsConfigurationFileRemaining);
        }

        private static AccountModel BuildAccount() => new()
        {
            SteamIdentifier = ValidSteamIdentifier,
            Label = "DummyUser"
        };

        private static AuthenticatorSecrets BuildSecrets() => new()
        {
            SharedSecret = ValidSharedSecret,
            IdentitySecret = ValidIdentitySecret
        };

        private sealed class StubConfigurationReader : DataAccess.Accounts.IAccountConfigurationReader
        {
            private readonly System.Collections.Generic.List<string> removedFiles = [];

            public AccountModel? Account { get; set; }

            public AuthenticatorSecrets? Secrets { get; set; }

            public string FailureReason { get; set; } = string.Empty;

            public bool CanBeRemoved { get; set; } = true;

            public bool WasRemoved => removedFiles.Count > 0;

            public bool Remove(string filePath)
            {
                if (CanBeRemoved)
                {
                    removedFiles.Add(filePath);
                }

                return CanBeRemoved;
            }

            public DataAccess.Accounts.AccountConfigurationReadResult Read(
                string filePath,
                out AccountModel? account,
                out AuthenticatorSecrets? secrets)
            {
                account = Account;
                secrets = Secrets;

                if (!string.IsNullOrWhiteSpace(FailureReason))
                {
                    account = null;
                    secrets = null;

                    return DataAccess.Accounts.AccountConfigurationReadResult.Failed(FailureReason);
                }

                return DataAccess.Accounts.AccountConfigurationReadResult.Successful();
            }
        }
    }
}
