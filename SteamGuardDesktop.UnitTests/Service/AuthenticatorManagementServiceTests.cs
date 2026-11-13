using NUnit.Framework;

using SteamGuardDesktop.Client.Steam;
using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Domain.Login;
using SteamGuardDesktop.Service.Authenticator;
using SteamGuardDesktop.Service.Time;
using SteamGuardDesktop.UnitTests.Utils;

namespace SteamGuardDesktop.UnitTests.Service
{
    [TestFixture]
    public sealed class AuthenticatorManagementServiceTests
    {
        private const string ValidSteamIdentifier = "76561198028572203";

        private const string ValidSharedSecret = "DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3";

        private const string ValidAccessToken = "access-token-value";

        private static readonly EnrolledAuthenticator EnrolledAuthenticator = new()
        {
            SteamIdentifier = ValidSteamIdentifier,
            SharedSecret = ValidSharedSecret,
            IdentitySecret = "GQP46b73Ws7gr8GmZFR0sDuau5c=",
            RevocationCode = "R61387",
            DeviceIdentifier = "186afecb275bf6db9bb855caa9c916dc56b2dc6a"
        };

        [Test]
        public void GivenAnAuthenticatorToActivate_WhenTheLocalClockDisagreesWithSteam_ThenActivationIsRefused()
        {
            StubAuthenticationClient client = new();
            AuthenticatorManagementService service = BuildService(client, new OutOfWindowClockDriftService());

            AuthenticatorOperationResult result = service.FinalizeAuthenticator(
                ValidSteamIdentifier,
                ValidAccessToken,
                EnrolledAuthenticator,
                "SMS01");

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(client.FinalizeWasCalled, Is.False);
        }

        [Test]
        public void GivenAnAuthenticatorToActivate_WhenTheClockIsSynchronised_ThenTheActivationSucceeds()
        {
            StubAuthenticationClient client = new();
            AuthenticatorManagementService service = BuildService(client, new SynchronisedClockDriftService());

            AuthenticatorOperationResult result = service.FinalizeAuthenticator(
                ValidSteamIdentifier,
                ValidAccessToken,
                EnrolledAuthenticator,
                "SMS01");

            Assert.That(result.IsSuccessful);
            Assert.That(client.FinalizeWasCalled);
        }

        [Test]
        public void GivenAnAuthenticatorToActivate_WhenSteamWantsAnotherAttempt_ThenTheActivationIsRetried()
        {
            StubAuthenticationClient client = new() { AttemptsBeforeSuccess = 2 };
            AuthenticatorManagementService service = BuildService(client, new SynchronisedClockDriftService());

            AuthenticatorOperationResult result = service.FinalizeAuthenticator(
                ValidSteamIdentifier,
                ValidAccessToken,
                EnrolledAuthenticator,
                "SMS01");

            Assert.That(result.IsSuccessful);
            Assert.That(client.FinalizeAttempts, Is.EqualTo(2));
        }

        [Test]
        public void GivenALostDevice_WhenStartingTheRecovery_ThenTheChallengeIsStarted()
        {
            StubAuthenticationClient client = new();
            AuthenticatorManagementService service = BuildService(client, new SynchronisedClockDriftService());

            AuthenticatorOperationResult result = service.StartLostDeviceRecovery(
                ValidSteamIdentifier,
                ValidAccessToken);

            Assert.That(result.IsSuccessful);
            Assert.That(client.StartRevocationWasCalled);
        }

        private static AuthenticatorManagementService BuildService(
            StubAuthenticationClient client,
            IClockDriftService clockDriftService) => new(
                client,
                new DeviceIdentityProvider(),
                new OtpCodeGenerator(
                    new SteamGuard.TOTP.SteamGuard(),
                    new FixedClock(System.DateTimeOffset.UnixEpoch)),
                clockDriftService,
                new NuciLog.Core.NullLogger());

        private sealed class SynchronisedClockDriftService : IClockDriftService
        {
            public ClockDriftReport? Evaluate() => new() { IsSynchronised = true };
        }

        private sealed class OutOfWindowClockDriftService : IClockDriftService
        {
            public ClockDriftReport? Evaluate() => new()
            {
                IsSynchronised = false,
                Offset = System.TimeSpan.FromSeconds(90)
            };
        }

        private sealed class StubAuthenticationClient : ISteamAuthenticationClient
        {
            public int AttemptsBeforeSuccess { get; set; } = 1;

            public int FinalizeAttempts { get; private set; }

            public bool FinalizeWasCalled { get; private set; }

            public bool StartRevocationWasCalled { get; private set; }

            public SteamClientResult<LoginRsaKey> GetPasswordRsaKey(string accountName) =>
                SteamClientResult<LoginRsaKey>.Failed("Not supported by this stub.");

            public SteamClientResult<LoginChallenge> BeginCredentialSession(
                LoginCredentials credentials,
                string encryptedPassword,
                long encryptionTimestamp) => SteamClientResult<LoginChallenge>.Failed("Not supported by this stub.");

            public SteamClientResult<bool> SubmitGuardCode(
                LoginChallenge challenge,
                string guardCode,
                LoginGuardType guardType) => SteamClientResult<bool>.Failed("Not supported by this stub.");

            public SteamClientResult<LoginTokens> PollSession(LoginChallenge challenge) =>
                SteamClientResult<LoginTokens>.Failed("Not supported by this stub.");

            public SteamClientResult<AuthenticatorStatus> GetAuthenticatorStatus(
                string accessToken,
                string steamIdentifier) => SteamClientResult<AuthenticatorStatus>.Failed("Not supported by this stub.");

            public SteamClientResult<bool> RemoveAuthenticator(
                string accessToken,
                string steamIdentifier,
                string revocationCode) => SteamClientResult<bool>.Failed("Not supported by this stub.");

            public SteamClientResult<EnrolledAuthenticator> AddAuthenticator(
                string accessToken,
                string steamIdentifier,
                string deviceIdentifier) => SteamClientResult<EnrolledAuthenticator>.Failed("Not supported by this stub.");

            public SteamClientResult<AuthenticatorFinalizationState> FinalizeAuthenticator(
                string accessToken,
                string steamIdentifier,
                string activationCode,
                string deviceIdentifier,
                string smsActivationCode)
            {
                FinalizeWasCalled = true;
                FinalizeAttempts += 1;

                if (FinalizeAttempts < AttemptsBeforeSuccess)
                {
                    return SteamClientResult<AuthenticatorFinalizationState>.Successful(
                        AuthenticatorFinalizationState.WantsMore);
                }

                return SteamClientResult<AuthenticatorFinalizationState>.Successful(
                    AuthenticatorFinalizationState.Succeeded);
            }

            public SteamClientResult<bool> StartAuthenticatorRevocation(
                string steamIdentifier,
                string accessToken)
            {
                StartRevocationWasCalled = true;

                return SteamClientResult<bool>.Successful(true);
            }

            public SteamClientResult<EnrolledAuthenticator> ContinueAuthenticatorRevocation(
                string accessToken,
                string smsCode,
                string steamIdentifier) => SteamClientResult<EnrolledAuthenticator>.Failed("Not supported by this stub.");

            public SteamClientResult<bool> RemoveAllGuardCookies(string accessToken) =>
                SteamClientResult<bool>.Failed("Not supported by this stub.");
        }
    }
}
