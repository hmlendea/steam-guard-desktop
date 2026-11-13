using System.Collections.Generic;
using System.Net.Http;

using NuciLog.Core;
using NUnit.Framework;

using SteamGuardDesktop.Client.Steam;
using SteamGuardDesktop.Configuration;
using SteamGuardDesktop.Domain.Login;
using SteamGuardDesktop.IntegrationTests.Utils;
using SteamGuardDesktop.Service.Diagnostics;

namespace SteamGuardDesktop.IntegrationTests.Steam
{
    [TestFixture]
    public sealed class SteamAuthenticationClientTests
    {
        private static string KeyPayload =>
            "{\"response\":{\"publickey_mod\":\"B1C4\",\"publickey_exp\":\"10001\",\"timestamp\":\"1617591917\"}}";

        private static string ChallengePayload =>
            "{\"response\":{\"client_id\":\"613\",\"request_id\":\"873\",\"interval\":5.0," +
            "\"steamid\":\"76561198028572203\",\"allowed_confirmations\":[{\"confirmation_type\":2," +
            "\"associated_message\":\"d***y@example.com\"}]}}";

        private static string TokensPayload =>
            "{\"response\":{\"steamid\":\"76561198028572203\",\"access_token\":\"access-token-value\"," +
            "\"refresh_token\":\"refresh-token-value\"}}";

        [Test]
        public void GivenAnAccountName_WhenRequestingThePasswordKey_ThenTheKeyedEndpointIsUsed()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(KeyPayload)
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<LoginRsaKey> result = client.GetPasswordRsaKey("DummyUser");

            Assert.That(result.IsSuccessful);
            Assert.That(result.Value?.Modulus, Is.EqualTo("B1C4"));
            Assert.That(result.Value?.Timestamp, Is.EqualTo(1617591917));
            Assert.That(handler.Requests[0].RequestUri?.AbsolutePath, Is.EqualTo("/IAuthenticationService/GetPasswordRSAPublicKey/v1"));
            Assert.That(handler.Requests[0].RequestUri?.Query, Does.Contain("input_json="));
        }

        [Test]
        public void GivenCredentials_WhenStartingASignIn_ThenTheChallengeReportsTheAcceptedGuards()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(ChallengePayload)
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<LoginChallenge> result = client.BeginCredentialSession(
                new LoginCredentials { AccountName = "DummyUser", Password = "dummy-password" },
                "encrypted-password",
                1617591917);

            Assert.That(result.IsSuccessful);
            Assert.That(result.Value?.ClientId, Is.EqualTo("613"));
            Assert.That(result.Value?.SteamIdentifier, Is.EqualTo("76561198028572203"));
            Assert.That(result.Value?.AcceptedGuards, Contains.Item(LoginGuardType.EmailCode));
            Assert.That(result.Value?.GuardMessage, Is.EqualTo("d***y@example.com"));
        }

        [Test]
        public void GivenASessionInProgress_WhenPollingIt_ThenTheIssuedTokensAreReturned()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(TokensPayload)
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<LoginTokens> result = client.PollSession(new LoginChallenge
            {
                ClientId = "613",
                RequestId = "873",
                PollingIntervalSeconds = 0
            });

            Assert.That(result.IsSuccessful);
            Assert.That(result.Value?.AccessToken, Is.EqualTo("access-token-value"));
            Assert.That(result.Value?.HasRefreshToken, Is.True);
        }

        [Test]
        public void GivenRejectedCredentials_WhenStartingASignIn_ThenTheRequestFailsWithTheReportedMessage()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(
                    "{\"response\":{\"extended_error_message\":\"The account name or password is incorrect.\"}}")
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<LoginChallenge> result = client.BeginCredentialSession(
                new LoginCredentials { AccountName = "DummyUser", Password = "dummy-password" },
                "encrypted-password",
                1617591917);

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.FailureReason, Does.Contain("incorrect"));
        }

        [Test]
        public void GivenAServerError_WhenStartingASignIn_ThenTheRequestFails()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateStatusResponse(System.Net.HttpStatusCode.InternalServerError)
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<LoginChallenge> result = client.BeginCredentialSession(
                new LoginCredentials { AccountName = "DummyUser", Password = "dummy-password" },
                "encrypted-password",
                1617591917);

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.FailureReason, Does.Contain("500"));
        }

        [Test]
        public void GivenAMalformedResponse_WhenPollingASession_ThenTheRequestFails()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse("{malformed")
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<LoginTokens> result = client.PollSession(new LoginChallenge
            {
                ClientId = "613",
                RequestId = "873",
                PollingIntervalSeconds = 0
            });

            Assert.That(result.IsSuccessful, Is.False);
        }

        [Test]
        public void GivenAnAuthenticator_WhenRemovingIt_ThenTheRevocationCodeIsSubmitted()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse("{\"response\":{\"success\":true}}")
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<bool> result = client.RemoveAuthenticator(
                "access-token-value",
                "76561198028572203",
                "revocation-code");

            Assert.That(result.IsSuccessful);
            Assert.That(handler.Requests[0].RequestUri?.AbsolutePath, Is.EqualTo("/ITwoFactorService/RemoveAuthenticator/v1"));
            Assert.That(handler.Requests[0].Headers.Authorization?.Parameter, Is.EqualTo("access-token-value"));
        }

        [Test]
        public void GivenAnAuthenticatorBeingAdded_WhenSteamReturnsItsSecrets_ThenTheyAreDecoded()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(
                    "{\"response\":{\"shared_secret\":\"c2hhcmVkLXNlY3JldA==\",\"identity_secret\":\"aWRlbnRpdHktc2VjcmV0\"," +
                    "\"revocation_code\":\"R61387\",\"device_identifier\":\"android:76561198028572203\"}}")
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<EnrolledAuthenticator> result = client.AddAuthenticator(
                "access-token-value",
                "76561198028572203",
                "android:76561198028572203");

            Assert.That(result.IsSuccessful);
            Assert.That(result.Value?.SharedSecret, Is.EqualTo("ONUGC4TFMQWXGZLDOJSXI"));
            Assert.That(result.Value?.IdentitySecret, Is.EqualTo("aWRlbnRpdHktc2VjcmV0"));
            Assert.That(result.Value?.RevocationCode, Is.EqualTo("R61387"));
        }

        [Test]
        public void GivenAnAuthenticatorWithoutSecrets_WhenAddingIt_ThenTheRequestFails()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse("{\"response\":{\"status\":2}}")
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<EnrolledAuthenticator> result = client.AddAuthenticator(
                "access-token-value",
                "76561198028572203",
                "android:76561198028572203");

            Assert.That(result.IsSuccessful, Is.False);
        }

        [Test]
        public void GivenALostDevice_WhenRecoveringIt_ThenTheReplacementAuthenticatorIsReturned()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(
                    "{\"response\":{\"success\":true,\"shared_secret\":\"c2hhcmVkLXNlY3JldA==\"," +
                    "\"identity_secret\":\"aWRlbnRpdHktc2VjcmV0\",\"revocation_code\":\"R61387\"," +
                    "\"steamguard_scheme\":2,\"steamid\":\"76561198028572203\"}}")
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<EnrolledAuthenticator> result = client.ContinueAuthenticatorRevocation(
                "access-token-value",
                "SMS01",
                "76561198028572203");

            Assert.That(result.IsSuccessful);
            Assert.That(result.Value?.SharedSecret, Is.EqualTo("ONUGC4TFMQWXGZLDOJSXI"));
            Assert.That(handler.Requests[0].RequestUri?.AbsolutePath, Is.EqualTo("/ITwoFactorService/RemoveAuthenticatorViaChallengeContinue/v1"));
        }

        [Test]
        public void GivenARejectedRecoveryCode_WhenRecoveringALostDevice_ThenTheRequestFails()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse("{\"response\":{\"success\":false}}")
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<EnrolledAuthenticator> result = client.ContinueAuthenticatorRevocation(
                "access-token-value",
                "SMS01",
                "76561198028572203");

            Assert.That(result.IsSuccessful, Is.False);
        }

        [Test]
        public void GivenAnActivationAttempt_WhenSteamAcceptsIt_ThenTheSmsCodeIsSubmittedAndSuccessIsReported()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse("{\"response\":{\"success\":true}}")
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<AuthenticatorFinalizationState> result = client.FinalizeAuthenticator(
                "access-token-value",
                "76561198028572203",
                "ABC12",
                "android:76561198028572203",
                "SMS01");

            Assert.That(result.IsSuccessful);
            Assert.That(result.Value, Is.EqualTo(AuthenticatorFinalizationState.Succeeded));

            string decodedBody = System.Net.WebUtility.UrlDecode(handler.RequestBodies[0]);

            Assert.That(decodedBody, Does.Contain("\"activation_code\":\"SMS01\""));
            Assert.That(decodedBody, Does.Contain("\"validate_sms_code\":\"1\""));
            Assert.That(decodedBody, Does.Contain("\"authenticator_code\":\"ABC12\""));
        }

        [Test]
        public void GivenAnActivationAttempt_WhenSteamWantsAnotherAttempt_ThenMoreIsReported()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse("{\"response\":{\"success\":false,\"want_more\":true}}")
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<AuthenticatorFinalizationState> result = client.FinalizeAuthenticator(
                "access-token-value",
                "76561198028572203",
                "ABC12",
                "android:76561198028572203",
                "SMS01");

            Assert.That(result.IsSuccessful);
            Assert.That(result.Value, Is.EqualTo(AuthenticatorFinalizationState.WantsMore));
        }

        [Test]
        public void GivenAnActivationAttempt_WhenSteamRejectsIt_ThenTheRequestFails()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse("{\"response\":{\"success\":false}}")
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<AuthenticatorFinalizationState> result = client.FinalizeAuthenticator(
                "access-token-value",
                "76561198028572203",
                "ABC12",
                "android:76561198028572203",
                "SMS01");

            Assert.That(result.IsSuccessful, Is.False);
        }

        [Test]
        public void GivenAnAuthenticator_WhenReadingItsStatus_ThenTheReportedStateIsMapped()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(
                    "{\"response\":{\"authenticator_type\":1,\"steamguard_scheme\":2," +
                    "\"device_identifier\":\"android:76561198028572203\",\"revocation_attempts_remaining\":3," +
                    "\"authenticator_allowed\":true}}")
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<AuthenticatorStatus> result = client.GetAuthenticatorStatus(
                "access-token-value",
                "76561198028572203");

            Assert.That(result.IsSuccessful);
            Assert.That(result.Value?.HasMobileAuthenticator, Is.True);
            Assert.That(result.Value?.RevocationAttemptsRemaining, Is.EqualTo(3));
        }

        [Test]
        public void GivenAnAccountWithoutAnAuthenticator_WhenReadingItsStatus_ThenNoAuthenticatorIsReported()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse("{\"response\":{\"authenticator_type\":0}}")
            ]);

            SteamAuthenticationClient client = BuildClient(handler);
            SteamClientResult<AuthenticatorStatus> result = client.GetAuthenticatorStatus(
                "access-token-value",
                "76561198028572203");

            Assert.That(result.IsSuccessful);
            Assert.That(result.Value?.HasMobileAuthenticator, Is.False);
        }

        [Test]
        public void GivenAFailure_WhenSendingARequest_ThenNoSecretAppearsInDiagnostics()
        {
            RecordingHttpMessageHandler handler = new(new HttpRequestException("The host is unreachable."));

            RecordingSecretRedactor redactor = new();
            SteamAuthenticationClient client = BuildClient(handler, redactor);

            client.GetPasswordRsaKey("DummyUser");

            Assert.That(redactor.SanitisedAddresses, Is.Not.Empty);
            Assert.That(redactor.SanitisedAddresses, Has.None.Contains("input_json"));
        }

        private static SteamAuthenticationClient BuildClient(RecordingHttpMessageHandler handler)
            => BuildClient(handler, new SecretRedactor());

        private static SteamAuthenticationClient BuildClient(
            RecordingHttpMessageHandler handler,
            ISecretRedactor redactor) => new(
            new HttpClient(handler),
            new SteamResponseParser(),
            new SteamRequestPayloadBuilder(),
            redactor,
            new NullLogger(),
            new SteamGuardSettings());
    }
}
