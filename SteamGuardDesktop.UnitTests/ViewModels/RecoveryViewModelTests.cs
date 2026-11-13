using System;
using System.Collections.Generic;

using NUnit.Framework;

using SteamGuardDesktop.Client.Steam;
using SteamGuardDesktop.Domain.Login;
using SteamGuardDesktop.Service.Account;
using SteamGuardDesktop.Service.Authenticator;
using SteamGuardDesktop.Service.Login;
using SteamGuardDesktop.ViewModels;

namespace SteamGuardDesktop.UnitTests.Presentation
{
    [TestFixture]
    public sealed class RecoveryViewModelTests
    {
        private const string ValidSteamIdentifier = "76561198028572203";

        private StubLoginService loginService = null!;

        private StubAuthenticatorManagementService managementService = null!;

        private StubAccountService accountService = null!;

        private RecoveryViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            loginService = new StubLoginService();
            managementService = new StubAuthenticatorManagementService();
            accountService = new StubAccountService();

            viewModel = new RecoveryViewModel(loginService, managementService, accountService);
        }

        [Test]
        public void GivenAChallengeThatAcceptsAnEmailCode_WhenStartingASignIn_ThenACodeIsRequired()
        {
            loginService.ChallengeToReturn = BuildChallenge([LoginGuardType.EmailCode]);

            viewModel.AccountName = "DummyUser";
            viewModel.Password = "dummy-password";
            viewModel.BeginSignIn();

            Assert.That(viewModel.IsSignInStarted);
            Assert.That(viewModel.IsCodeRequired);
        }

        [Test]
        public void GivenAChallengeThatOnlyAcceptsDeviceConfirmation_WhenStartingASignIn_ThenNoCodeIsRequired()
        {
            loginService.ChallengeToReturn = BuildChallenge([LoginGuardType.DeviceConfirmation]);

            viewModel.AccountName = "DummyUser";
            viewModel.Password = "dummy-password";
            viewModel.BeginSignIn();

            Assert.That(viewModel.IsSignInStarted);
            Assert.That(viewModel.IsCodeRequired, Is.False);
        }

        [Test]
        public void GivenADeviceCodeChallenge_WhenSubmittingACode_ThenTheDeviceGuardTypeIsUsed()
        {
            loginService.ChallengeToReturn = BuildChallenge([LoginGuardType.DeviceCode]);
            viewModel.AccountName = "DummyUser";
            viewModel.Password = "dummy-password";
            viewModel.BeginSignIn();

            viewModel.GuardCode = "ABC12";
            viewModel.SubmitGuardCode();

            Assert.That(loginService.SubmittedGuardType, Is.EqualTo(LoginGuardType.DeviceCode));
        }

        [Test]
        public void GivenADeviceConfirmationChallenge_WhenSubmittingACode_ThenNothingIsSubmitted()
        {
            loginService.ChallengeToReturn = BuildChallenge([LoginGuardType.DeviceConfirmation]);
            viewModel.AccountName = "DummyUser";
            viewModel.Password = "dummy-password";
            viewModel.BeginSignIn();

            viewModel.GuardCode = "ABC12";
            viewModel.SubmitGuardCode();

            Assert.That(loginService.SubmittedGuardType, Is.EqualTo(LoginGuardType.Unsupported));
            Assert.That(viewModel.HasStatusMessage);
        }

        [Test]
        public void GivenAStartedSignIn_WhenStartingIt_ThenThePasswordIsClearedImmediately()
        {
            loginService.ChallengeToReturn = BuildChallenge([LoginGuardType.EmailCode]);

            viewModel.AccountName = "DummyUser";
            viewModel.Password = "dummy-password";
            viewModel.BeginSignIn();

            Assert.That(viewModel.Password, Is.Empty);
        }

        [Test]
        public void GivenACompletedSignIn_WhenTheTokensAreIssued_ThenTheAccessTokenIsStored()
        {
            viewModel.AccountName = "DummyUser";
            viewModel.SteamIdentifier = ValidSteamIdentifier;
            viewModel.AccessToken = "access-token-value";

            CompleteSignIn();

            Assert.That(accountService.SavedSteamIdentifier, Is.EqualTo(ValidSteamIdentifier));
            Assert.That(accountService.SavedAccessToken, Is.EqualTo("access-token-value"));
        }

        [Test]
        public void GivenACompletedSignIn_WhenTheTokenCannotBeStored_ThenTheFailureIsReported()
        {
            accountService.IsTokenSaveSuccessful = false;

            CompleteSignIn();

            Assert.That(viewModel.HasStatusMessage);
        }

        [Test]
        public void GivenNoSignIn_WhenEnrollingAnAuthenticator_ThenTheRequestIsRejected()
        {
            viewModel.EnrolAuthenticator();

            Assert.That(viewModel.HasStatusMessage);
            Assert.That(managementService.AddWasCalled, Is.False);
        }

        [Test]
        public void GivenASignedInAccount_WhenEnrollingAnAuthenticator_ThenTheAccountIsSaved()
        {
            viewModel.AccountName = "DummyUser";
            viewModel.SteamIdentifier = ValidSteamIdentifier;
            viewModel.AccessToken = "access-token-value";

            managementService.AuthenticatorToReturn = BuildEnrolledAuthenticator();

            viewModel.EnrolAuthenticator();
            viewModel.FinalizeAuthenticator();

            Assert.That(managementService.FinalizeWasCalled);
            Assert.That(accountService.SavedAccount?.SteamIdentifier, Is.EqualTo(ValidSteamIdentifier));
        }

        [Test]
        public void GivenNoSignIn_WhenRecoveringALostDevice_ThenTheRequestIsRejected()
        {
            viewModel.RecoverLostDevice();

            Assert.That(viewModel.HasStatusMessage);
            Assert.That(managementService.RecoverWasCalled, Is.False);
        }

        [Test]
        public void GivenASignedInAccount_WhenRecoveringALostDevice_ThenTheRecoveredAccountIsSaved()
        {
            viewModel.AccountName = "DummyUser";
            viewModel.SteamIdentifier = ValidSteamIdentifier;
            viewModel.AccessToken = "access-token-value";
            viewModel.SmsCode = "SMS01";

            managementService.AuthenticatorToReturn = BuildEnrolledAuthenticator();

            viewModel.RecoverLostDevice();

            Assert.That(managementService.RecoverWasCalled);
            Assert.That(accountService.SavedAccount?.SteamIdentifier, Is.EqualTo(ValidSteamIdentifier));
        }

        [Test]
        public void GivenAPopulatedForm_WhenResetting_ThenEveryFieldIsCleared()
        {
            viewModel.AccountName = "DummyUser";
            viewModel.Password = "dummy-password";
            viewModel.GuardCode = "ABC12";
            viewModel.SmsCode = "SMS01";
            viewModel.RecoveryCode = "revocation-code";
            viewModel.AccessToken = "access-token-value";
            viewModel.SteamIdentifier = ValidSteamIdentifier;

            viewModel.Reset();

            Assert.That(viewModel.AccountName, Is.Empty);
            Assert.That(viewModel.Password, Is.Empty);
            Assert.That(viewModel.GuardCode, Is.Empty);
            Assert.That(viewModel.SmsCode, Is.Empty);
            Assert.That(viewModel.RecoveryCode, Is.Empty);
            Assert.That(viewModel.AccessToken, Is.Empty);
            Assert.That(viewModel.SteamIdentifier, Is.Empty);
        }

        [Test]
        public void GivenARejectedCredentialSignIn_WhenStartingIt_ThenTheReasonIsReported()
        {
            loginService.FailureReason = "The account name or password is incorrect.";

            viewModel.AccountName = "Grumpy Cat";
            viewModel.Password = "wrong-password";
            viewModel.BeginSignIn();

            Assert.That(viewModel.IsSignInStarted, Is.False);
            Assert.That(viewModel.StatusMessage, Does.Contain("incorrect"));
        }

        private void CompleteSignIn()
        {
            loginService.ChallengeToReturn = BuildChallenge([LoginGuardType.EmailCode]);
            loginService.TokensToReturn = new LoginTokens
            {
                SteamIdentifier = ValidSteamIdentifier,
                AccessToken = "access-token-value"
            };

            viewModel.BeginSignIn();
            viewModel.CompleteSignIn();
        }

        private static LoginChallenge BuildChallenge(IEnumerable<LoginGuardType> acceptedGuards) => new()
        {
            ClientId = "613",
            RequestId = "873",
            SteamIdentifier = ValidSteamIdentifier,
            AcceptedGuards = acceptedGuards,
            GuardMessage = "d***y@example.com"
        };

        private static EnrolledAuthenticator BuildEnrolledAuthenticator() => new()
        {
            SteamIdentifier = ValidSteamIdentifier,
            SharedSecret = "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3",
            IdentitySecret = "GQP46b73Ws7gr8GmZFR0sDuau5c=",
            RevocationCode = "R61387",
            DeviceIdentifier = "android:76561198028572203"
        };

        private sealed class StubLoginService : ILoginService
        {
            public LoginChallenge? ChallengeToReturn { get; set; }

            public LoginTokens? TokensToReturn { get; set; }

            public string FailureReason { get; set; } = string.Empty;

            public LoginGuardType SubmittedGuardType { get; private set; }

            public LoginStartResult BeginLogin(LoginCredentials credentials)
            {
                if (!string.IsNullOrWhiteSpace(FailureReason) || ChallengeToReturn is null)
                {
                    return LoginStartResult.Failed(FailureReason);
                }

                return LoginStartResult.Successful(ChallengeToReturn);
            }

            public LoginGuardResult SubmitGuardCode(
                LoginChallenge challenge,
                string guardCode,
                LoginGuardType guardType)
            {
                SubmittedGuardType = guardType;

                return LoginGuardResult.Successful();
            }

            public LoginCompletionResult CompleteLogin(LoginChallenge challenge)
            {
                if (TokensToReturn is null)
                {
                    return LoginCompletionResult.Failed("No tokens were issued.");
                }

                return LoginCompletionResult.Successful(TokensToReturn);
            }
        }

        private sealed class StubAuthenticatorManagementService : IAuthenticatorManagementService
        {
            public EnrolledAuthenticator? AuthenticatorToReturn { get; set; }

            public bool AddWasCalled { get; private set; }

            public bool FinalizeWasCalled { get; private set; }

            public bool RecoverWasCalled { get; private set; }

            public AuthenticatorStatusResult GetStatus(string steamIdentifier, string accessToken) =>
                AuthenticatorStatusResult.Successful(new AuthenticatorStatus { HasMobileAuthenticator = true });

            public AuthenticatorOperationResult RemoveAuthenticator(
                string steamIdentifier,
                string accessToken,
                string revocationCode) => AuthenticatorOperationResult.Successful();

            public AuthenticatorEnrolmentResult AddAuthenticator(string steamIdentifier, string accessToken)
            {
                AddWasCalled = true;

                if (AuthenticatorToReturn is null)
                {
                    return AuthenticatorEnrolmentResult.Failed("Steam did not issue an authenticator.");
                }

                return AuthenticatorEnrolmentResult.Successful(AuthenticatorToReturn, "ABC12");
            }

            public AuthenticatorOperationResult FinalizeAuthenticator(
                string steamIdentifier,
                string accessToken,
                string activationCode,
                string deviceIdentifier)
            {
                FinalizeWasCalled = true;

                return AuthenticatorOperationResult.Successful();
            }

            public AuthenticatorEnrolmentResult RecoverAuthenticator(
                string steamIdentifier,
                string accessToken,
                string smsCode)
            {
                RecoverWasCalled = true;

                if (AuthenticatorToReturn is null)
                {
                    return AuthenticatorEnrolmentResult.Failed("Steam rejected the recovery code.");
                }

                return AuthenticatorEnrolmentResult.Successful(AuthenticatorToReturn, string.Empty);
            }

            public SteamGuardDesktop.Domain.Authenticator.AuthenticatorSecrets CreateSecrets(EnrolledAuthenticator enrolledAuthenticator) => new()
            {
                SharedSecret = enrolledAuthenticator.SharedSecret,
                IdentitySecret = enrolledAuthenticator.IdentitySecret,
                RevocationCode = enrolledAuthenticator.RevocationCode
            };

            public string? GenerateActivationCode(EnrolledAuthenticator enrolledAuthenticator) => "ABC12";
        }

        private sealed class StubAccountService : IAccountService
        {
            public SteamGuardDesktop.Domain.Account.SteamAccount? SavedAccount { get; private set; }

            public string SavedSteamIdentifier { get; private set; } = string.Empty;

            public string SavedAccessToken { get; private set; } = string.Empty;

            public bool IsTokenSaveSuccessful { get; set; } = true;

            public IEnumerable<SteamGuardDesktop.Domain.Account.SteamAccount> GetAccounts() => [];

            public AccountSaveResult SaveAccount(
                SteamGuardDesktop.Domain.Account.SteamAccount account,
                SteamGuardDesktop.Domain.Authenticator.AuthenticatorSecrets secrets)
            {
                SavedAccount = account;

                return AccountSaveResult.Successful(account);
            }

            public AccountRemovalResult RemoveAccount(string accountIdentifier) => AccountRemovalResult.Successful();

            public SteamGuardDesktop.Domain.Authenticator.AuthenticatorSecrets? GetSecrets(SteamGuardDesktop.Domain.Account.SteamAccount account) => null;

            public AccountAccessTokenResult SaveAccessToken(string steamIdentifier, string accessToken)
            {
                SavedSteamIdentifier = steamIdentifier;
                SavedAccessToken = accessToken;

                if (!IsTokenSaveSuccessful)
                {
                    return AccountAccessTokenResult.Failed("The token could not be stored.");
                }

                return AccountAccessTokenResult.Successful();
            }

            public SteamGuardDesktop.Service.Storage.SecretStoreAvailability GetSecretStoreAvailability() =>
                SteamGuardDesktop.Service.Storage.SecretStoreAvailability.Available;
        }
    }
}
