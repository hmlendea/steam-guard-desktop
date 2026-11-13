using System.Collections.Generic;

using NUnit.Framework;

using SteamGuardDesktop.Configuration;
using SteamGuardDesktop.Domain.Login;
using SteamGuardDesktop.Service.Account;
using SteamGuardDesktop.Service.Authenticator;
using SteamGuardDesktop.Service.Login;

using RecoveryModel = SteamGuardDesktop.ViewModels.RecoveryViewModel;

namespace SteamGuardDesktop.UnitTests.Presentation
{
    [TestFixture]
    public sealed class RecoveryNotificationsTests
    {
        [Test]
        public void GivenAStubSignIn_WhenACodeBecomesRequired_ThenTheChangeIsAnnounced()
        {
            StubLoginService loginService = new();
            RecoveryModel viewModel = new(
                loginService,
                new StubAuthenticatorManagementService(),
                new StubAccountService());

            List<string> changedProperties = [];
            viewModel.PropertyChanged += (sender, arguments) =>
            {
                if (arguments.PropertyName is not null)
                {
                    changedProperties.Add(arguments.PropertyName);
                }
            };

            loginService.ChallengeToReturn = new LoginChallenge
            {
                ClientId = "613",
                RequestId = "873",
                SteamIdentifier = "76561198028572203",
                AcceptedGuards = [LoginGuardType.EmailCode]
            };

            viewModel.AccountName = "DummyUser";
            viewModel.Password = "dummy-password";
            viewModel.BeginSignIn();

            Assert.That(changedProperties, Contains.Item("IsGuardCodeRequired"));
            Assert.That(changedProperties, Contains.Item("IsCodeRequired"));
        }

        private sealed class StubLoginService : ILoginService
        {
            public LoginChallenge? ChallengeToReturn { get; set; }

            public LoginStartResult BeginLogin(LoginCredentials credentials) =>
                ChallengeToReturn is null
                    ? LoginStartResult.Failed("The sign-in could not be started.")
                    : LoginStartResult.Successful(ChallengeToReturn);

            public LoginGuardResult SubmitGuardCode(
                LoginChallenge challenge,
                string guardCode,
                LoginGuardType guardType) => LoginGuardResult.Successful();

            public LoginCompletionResult CompleteLogin(LoginChallenge challenge) =>
                LoginCompletionResult.Failed("The sign-in is not complete.");
        }

        private sealed class StubAuthenticatorManagementService : IAuthenticatorManagementService
        {
            public AuthenticatorStatusResult GetStatus(string steamIdentifier, string accessToken) =>
                AuthenticatorStatusResult.Successful(new AuthenticatorStatus());

            public AuthenticatorOperationResult RemoveAuthenticator(
                string steamIdentifier,
                string accessToken,
                string revocationCode) => AuthenticatorOperationResult.Successful();

            public AuthenticatorEnrolmentResult AddAuthenticator(string steamIdentifier, string accessToken) =>
                AuthenticatorEnrolmentResult.Failed("Not supported by this stub.");

            public AuthenticatorOperationResult FinalizeAuthenticator(
                string steamIdentifier,
                string accessToken,
                string activationCode,
                string deviceIdentifier) => AuthenticatorOperationResult.Successful();

            public AuthenticatorEnrolmentResult RecoverAuthenticator(
                string steamIdentifier,
                string accessToken,
                string smsCode) => AuthenticatorEnrolmentResult.Failed("Not supported by this stub.");

            public AuthenticatorOperationResult RemoveAllGuardCookies(string accessToken) =>
                AuthenticatorOperationResult.Successful();

            public SteamGuardDesktop.Domain.Authenticator.AuthenticatorSecrets CreateSecrets(
                EnrolledAuthenticator enrolledAuthenticator) => new();

            public string? GenerateActivationCode(EnrolledAuthenticator enrolledAuthenticator) => null;
        }

        private sealed class StubAccountService : IAccountService
        {
            public IEnumerable<SteamGuardDesktop.Domain.Account.SteamAccount> GetAccounts() => [];

            public AccountSaveResult SaveAccount(
                SteamGuardDesktop.Domain.Account.SteamAccount account,
                SteamGuardDesktop.Domain.Authenticator.AuthenticatorSecrets secrets) =>
                AccountSaveResult.Successful(account);

            public AccountRemovalResult RemoveAccount(string accountIdentifier) => AccountRemovalResult.Successful();

            public AccountImportResult ImportAccount(string filePath) =>
                AccountImportResult.Failed("Not supported by this stub.");

            public bool IsAccountConfigured(string steamIdentifier) => true;

            public SteamGuardDesktop.Domain.Authenticator.AuthenticatorSecrets? GetSecrets(
                SteamGuardDesktop.Domain.Account.SteamAccount account) => null;

            public AccountAccessTokenResult SaveAccessToken(string steamIdentifier, string accessToken) =>
                AccountAccessTokenResult.Successful();

            public SteamGuardDesktop.Service.Storage.SecretStoreAvailability GetSecretStoreAvailability() =>
                SteamGuardDesktop.Service.Storage.SecretStoreAvailability.Available;
        }
    }
}
