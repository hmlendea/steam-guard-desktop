using System;
using System.Collections.Generic;

using NUnit.Framework;

using SteamGuardDesktop.Domain.Login;
using SteamGuardDesktop.Service.Account;
using SteamGuardDesktop.Service.Authenticator;
using SteamGuardDesktop.Service.Login;

using RecoveryModel = SteamGuardDesktop.ViewModels.RecoveryViewModel;

namespace SteamGuardDesktop.UnitTests.Presentation
{
    [TestFixture]
    public sealed class RecoveryAccountStateTests
    {
        private const string ValidSteamIdentifier = "76561198028572203";

        private StubLoginService loginService = null!;

        private StubAuthenticatorManagementService managementService = null!;

        private StubAccountService accountService = null!;

        private RecoveryModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            loginService = new StubLoginService();
            managementService = new StubAuthenticatorManagementService();
            accountService = new StubAccountService();

            viewModel = new RecoveryModel(loginService, managementService, accountService);
        }

        [Test]
        public void GivenANewAccount_WhenCompletingASignIn_ThenNoTokenIsStoredAndTheUserIsInformed()
        {
            accountService.IsConfigured = false;
            CompleteSignIn();

            Assert.That(accountService.SavedAccessToken, Is.Empty);
            Assert.That(viewModel.StatusMessage, Does.Contain("Enrol or recover"));
        }

        [Test]
        public void GivenAConfiguredAccount_WhenCompletingASignIn_ThenTheTokenIsStored()
        {
            accountService.IsConfigured = true;
            CompleteSignIn();

            Assert.That(accountService.SavedAccessToken, Is.EqualTo("access-token-value"));
        }

        [Test]
        public void GivenAConfiguredAccount_WhenASignInSucceeds_ThenTheAccountSavedEventIsNotRaised()
        {
            accountService.IsConfigured = true;
            bool wasRaised = false;
            viewModel.AccountSaved += (sender, arguments) => wasRaised = true;

            CompleteSignIn();

            Assert.That(wasRaised, Is.False);
        }

        [Test]
        public void GivenASignedInAccount_WhenRecoveringALostDevice_ThenTheAccountSavedEventIsRaised()
        {
            bool wasRaised = false;
            viewModel.AccountSaved += (sender, arguments) => wasRaised = true;

            viewModel.SteamIdentifier = ValidSteamIdentifier;
            viewModel.AccessToken = "access-token-value";
            viewModel.SmsCode = "SMS01";
            managementService.AuthenticatorToReturn = BuildEnrolledAuthenticator();

            viewModel.RecoverLostDevice();

            Assert.That(wasRaised);
        }

        [Test]
        public void GivenASignedInAccount_WhenRecoveringALostDevice_ThenTheTokenIsStoredAfterTheAccount()
        {
            viewModel.SteamIdentifier = ValidSteamIdentifier;
            viewModel.AccessToken = "access-token-value";
            viewModel.SmsCode = "SMS01";
            managementService.AuthenticatorToReturn = BuildEnrolledAuthenticator();

            viewModel.RecoverLostDevice();

            Assert.That(accountService.SavedAccount, Is.Not.Null);
            Assert.That(accountService.SavedAccessToken, Is.EqualTo("access-token-value"));
        }

        [Test]
        public void GivenASignedInAccount_WhenSigningOut_ThenTheSessionIsRevokedAndTheFormIsCleared()
        {
            viewModel.SteamIdentifier = ValidSteamIdentifier;
            viewModel.AccessToken = "access-token-value";
            viewModel.IsGuardSubmitted = true;

            viewModel.SignOut();

            Assert.That(managementService.WasSignedOut);
            Assert.That(viewModel.AccessToken, Is.Empty);
            Assert.That(viewModel.IsGuardSubmitted, Is.False);
            Assert.That(viewModel.SignOutRequestCount, Is.EqualTo(1));
        }

        [Test]
        public void GivenASignedInAccount_WhenSigningOutTwice_ThenTheRequestCountAdvances()
        {
            viewModel.AccessToken = "access-token-value";

            viewModel.SignOut();
            viewModel.SignOut();

            Assert.That(viewModel.SignOutRequestCount, Is.EqualTo(2));
        }

        private void CompleteSignIn()
        {
            loginService.ChallengeToReturn = BuildChallenge();
            loginService.TokensToReturn = new LoginTokens
            {
                SteamIdentifier = ValidSteamIdentifier,
                AccessToken = "access-token-value"
            };

            viewModel.AccountName = "DummyUser";
            viewModel.Password = "dummy-password";
            viewModel.BeginSignIn();
            viewModel.CompleteSignIn();
        }

        private static LoginChallenge BuildChallenge() => new()
        {
            ClientId = "613",
            RequestId = "873",
            SteamIdentifier = ValidSteamIdentifier,
            AcceptedGuards = [LoginGuardType.EmailCode]
        };

        private static EnrolledAuthenticator BuildEnrolledAuthenticator() => new()
        {
            SteamIdentifier = ValidSteamIdentifier,
            SharedSecret = "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3",
            IdentitySecret = "GQP46b73Ws7gr8GmZFR0sDuau5c=",
            RevocationCode = "R61387"
        };

        private sealed class StubLoginService : ILoginService
        {
            public LoginChallenge? ChallengeToReturn { get; set; }

            public LoginTokens? TokensToReturn { get; set; }

            public LoginStartResult BeginLogin(LoginCredentials credentials) =>
                ChallengeToReturn is null
                    ? LoginStartResult.Failed("The sign-in could not be started.")
                    : LoginStartResult.Successful(ChallengeToReturn);

            public LoginGuardResult SubmitGuardCode(
                LoginChallenge challenge,
                string guardCode,
                LoginGuardType guardType) => LoginGuardResult.Successful();

            public LoginCompletionResult CompleteLogin(LoginChallenge challenge) =>
                TokensToReturn is null
                    ? LoginCompletionResult.Failed("No tokens were issued.")
                    : LoginCompletionResult.Successful(TokensToReturn);
        }

        private sealed class StubAuthenticatorManagementService : IAuthenticatorManagementService
        {
            public EnrolledAuthenticator? AuthenticatorToReturn { get; set; }

            public bool WasSignedOut { get; private set; }

            public AuthenticatorStatusResult GetStatus(string steamIdentifier, string accessToken) =>
                AuthenticatorStatusResult.Successful(new AuthenticatorStatus { HasMobileAuthenticator = true });

            public AuthenticatorOperationResult RemoveAuthenticator(
                string steamIdentifier,
                string accessToken,
                string revocationCode) => AuthenticatorOperationResult.Successful();

            public AuthenticatorEnrolmentResult AddAuthenticator(string steamIdentifier, string accessToken) =>
                AuthenticatorToReturn is null
                    ? AuthenticatorEnrolmentResult.Failed("Steam did not issue an authenticator.")
                    : AuthenticatorEnrolmentResult.Successful(AuthenticatorToReturn, "ABC12");

            public AuthenticatorOperationResult FinalizeAuthenticator(
                string steamIdentifier,
                string accessToken,
                string activationCode,
                string deviceIdentifier) => AuthenticatorOperationResult.Successful();

            public AuthenticatorEnrolmentResult RecoverAuthenticator(
                string steamIdentifier,
                string accessToken,
                string smsCode) =>
                AuthenticatorToReturn is null
                    ? AuthenticatorEnrolmentResult.Failed("Steam rejected the recovery code.")
                    : AuthenticatorEnrolmentResult.Successful(AuthenticatorToReturn, string.Empty);

            public AuthenticatorOperationResult RemoveAllGuardCookies(string accessToken)
            {
                WasSignedOut = true;

                return AuthenticatorOperationResult.Successful();
            }

            public SteamGuardDesktop.Domain.Authenticator.AuthenticatorSecrets CreateSecrets(
                EnrolledAuthenticator enrolledAuthenticator) => new()
                {
                    SharedSecret = enrolledAuthenticator.SharedSecret,
                    IdentitySecret = enrolledAuthenticator.IdentitySecret,
                    RevocationCode = enrolledAuthenticator.RevocationCode
                };

            public string? GenerateActivationCode(EnrolledAuthenticator enrolledAuthenticator) => "ABC12";
        }

        private sealed class StubAccountService : IAccountService
        {
            public bool IsConfigured { get; set; } = true;

            public SteamGuardDesktop.Domain.Account.SteamAccount? SavedAccount { get; private set; }

            public string SavedAccessToken { get; private set; } = string.Empty;

            public IEnumerable<SteamGuardDesktop.Domain.Account.SteamAccount> GetAccounts() => [];

            public AccountSaveResult SaveAccount(
                SteamGuardDesktop.Domain.Account.SteamAccount account,
                SteamGuardDesktop.Domain.Authenticator.AuthenticatorSecrets secrets)
            {
                SavedAccount = account;
                IsConfigured = true;

                return AccountSaveResult.Successful(account);
            }

            public AccountRemovalResult RemoveAccount(string accountIdentifier) => AccountRemovalResult.Successful();

            public AccountImportResult ImportAccount(string filePath) =>
                AccountImportResult.Failed("Not supported by this stub.");

            public bool IsAccountConfigured(string steamIdentifier) => IsConfigured;

            public SteamGuardDesktop.Domain.Authenticator.AuthenticatorSecrets? GetSecrets(
                SteamGuardDesktop.Domain.Account.SteamAccount account) => null;

            public AccountAccessTokenResult SaveAccessToken(string steamIdentifier, string accessToken)
            {
                SavedAccessToken = accessToken;

                return AccountAccessTokenResult.Successful();
            }

            public SteamGuardDesktop.Service.Storage.SecretStoreAvailability GetSecretStoreAvailability() =>
                SteamGuardDesktop.Service.Storage.SecretStoreAvailability.Available;
        }
    }
}
