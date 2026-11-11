using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

using NUnit.Framework;

using NuciLog.Core;

using SteamGuardDesktop.Client.Steam;
using SteamGuardDesktop.Configuration;
using SteamGuardDesktop.Domain.Confirmations;
using SteamGuardDesktop.IntegrationTests.Utils;
using SteamGuardDesktop.Service.Confirmations;
using SteamGuardDesktop.Service.Diagnostics;

namespace SteamGuardDesktop.IntegrationTests.Steam
{
    [TestFixture]
    public sealed class SteamConfirmationClientTests
    {
        private static string IdentitySecret => "GQP46b73Ws7gr8GmZFR0sDuau5c=";

        private static string ServerTimePayload => "{\"response\":{\"server_time\":\"1617591917\"}}";

        private static string EmptyConfirmationPayload => "{\"success\":true,\"conf\":[]}";

        [Test]
        public void GivenAConfirmationList_WhenRequestingIt_ThenTheSignedQueryAndSessionCookieAreSent()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(ServerTimePayload),
                RecordingHttpMessageHandler.CreateJsonResponse(EmptyConfirmationPayload)
            ]);

            SteamConfirmationClient client = BuildClient(handler);

            SteamClientResult<IEnumerable<Confirmation>> result = client.GetConfirmations(BuildContext());

            Assert.That(result.IsSuccessful);
            Assert.That(handler.Requests, Has.Count.EqualTo(2));
            Assert.That(handler.Requests[1].RequestUri?.AbsolutePath, Is.EqualTo("/mobileconf/getlist"));
            Assert.That(handler.Requests[1].RequestUri?.Query, Does.Contain("tag=conf"));
            Assert.That(handler.Requests[1].RequestUri?.Query, Does.Contain("m=react"));
            Assert.That(handler.Requests[1].RequestUri?.Query, Does.Contain("a=76561198028572203"));
            Assert.That(
                handler.Requests[1].Headers.GetValues("Cookie"),
                Has.Some.Contains("steamLoginSecure=76561198028572203||"));
        }

        [Test]
        public void GivenAConfirmationListWithEntries_WhenRequestingIt_ThenTheEntriesAreMapped()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(ServerTimePayload),
                RecordingHttpMessageHandler.CreateJsonResponse(
                    """
                    {"success":true,"conf":[{"type":2,"type_name":"Trade Offer","id":"613","creator_id":"873","nonce":"nonce-value","creation_time":1617591900,"headline":"Trade offer","summary":["You receive","You give"]}]}
                    """)
            ]);

            SteamConfirmationClient client = BuildClient(handler);

            SteamClientResult<IEnumerable<Confirmation>> result = client.GetConfirmations(BuildContext());

            Assert.That(result.IsSuccessful);
            Assert.That(result.Value, Has.Exactly(1).Items);
            Assert.That(
                Enumerable.First(result.Value!).Type,
                Is.EqualTo(ConfirmationType.Trade));
            Assert.That(
                Enumerable.First(result.Value!).Summary,
                Is.EqualTo("You receive, You give"));
        }

        [Test]
        public void GivenASessionThatSteamRejects_WhenRequestingTheList_ThenAuthenticationIsRequired()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(ServerTimePayload),
                RecordingHttpMessageHandler.CreateJsonResponse("{\"success\":false,\"needauth\":true}")
            ]);

            SteamConfirmationClient client = BuildClient(handler);

            SteamClientResult<IEnumerable<Confirmation>> result = client.GetConfirmations(BuildContext());

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.RequiresAuthentication);
        }

        [Test]
        public void GivenAServerError_WhenRequestingTheList_ThenTheRequestFails()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(ServerTimePayload),
                RecordingHttpMessageHandler.CreateStatusResponse(HttpStatusCode.InternalServerError)
            ]);

            SteamConfirmationClient client = BuildClient(handler);

            SteamClientResult<IEnumerable<Confirmation>> result = client.GetConfirmations(BuildContext());

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.FailureReason, Does.Contain("500"));
        }

        [Test]
        public void GivenMalformedJson_WhenRequestingTheList_ThenTheRequestFails()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(ServerTimePayload),
                RecordingHttpMessageHandler.CreateJsonResponse("{not-json")
            ]);

            SteamConfirmationClient client = BuildClient(handler);

            SteamClientResult<IEnumerable<Confirmation>> result = client.GetConfirmations(BuildContext());

            Assert.That(result.IsSuccessful, Is.False);
        }

        [Test]
        public void GivenANetworkFailure_WhenRequestingTheList_ThenTheRequestFailsWithoutThrowing()
        {
            RecordingHttpMessageHandler handler = new(new HttpRequestException("The host is unreachable."));

            SteamConfirmationClient client = BuildClient(handler);

            SteamClientResult<IEnumerable<Confirmation>> result = client.GetConfirmations(BuildContext());

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.FailureReason, Is.Not.Empty);
        }

        [Test]
        public void GivenAnApprovalDecision_WhenApplyingIt_ThenTheAllowOperationAndConfirmationIdentifiersAreSent()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(ServerTimePayload),
                RecordingHttpMessageHandler.CreateJsonResponse("{\"success\":true}")
            ]);

            SteamConfirmationClient client = BuildClient(handler);

            ConfirmationDecision decision = new()
            {
                ConfirmationIdentifier = "613",
                Nonce = "nonce-value",
                DecisionType = ConfirmationDecisionType.Approve
            };

            SteamClientResult<bool> result = client.ApplyDecision(BuildContext(), decision);

            Assert.That(result.IsSuccessful);
            Assert.That(handler.Requests[1].RequestUri?.AbsolutePath, Is.EqualTo("/mobileconf/ajaxop"));
            Assert.That(handler.Requests[1].RequestUri?.Query, Does.Contain("op=allow"));
            Assert.That(handler.Requests[1].RequestUri?.Query, Does.Contain("cid=613"));
            Assert.That(handler.Requests[1].RequestUri?.Query, Does.Contain("ck=nonce-value"));
        }

        [Test]
        public void GivenADenialDecision_WhenApplyingIt_ThenTheCancelOperationIsSent()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(ServerTimePayload),
                RecordingHttpMessageHandler.CreateJsonResponse("{\"success\":true}")
            ]);

            SteamConfirmationClient client = BuildClient(handler);

            ConfirmationDecision decision = new()
            {
                ConfirmationIdentifier = "873",
                Nonce = "nonce-value",
                DecisionType = ConfirmationDecisionType.Deny
            };

            SteamClientResult<bool> result = client.ApplyDecision(BuildContext(), decision);

            Assert.That(result.IsSuccessful);
            Assert.That(handler.Requests[1].RequestUri?.Query, Does.Contain("op=cancel"));
        }

        [Test]
        public void GivenASessionThatSteamRejects_WhenApplyingADecision_ThenAuthenticationIsRequired()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(ServerTimePayload),
                RecordingHttpMessageHandler.CreateJsonResponse("{\"success\":false,\"needsauth\":true}")
            ]);

            SteamConfirmationClient client = BuildClient(handler);

            ConfirmationDecision decision = new()
            {
                ConfirmationIdentifier = "613",
                Nonce = "nonce-value",
                DecisionType = ConfirmationDecisionType.Approve
            };

            SteamClientResult<bool> result = client.ApplyDecision(BuildContext(), decision);

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.RequiresAuthentication);
        }

        [Test]
        public void GivenNoSignedQueryInDiagnostics_WhenRequestingTheList_ThenTheRequestAddressIsRedacted()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(ServerTimePayload),
                RecordingHttpMessageHandler.CreateJsonResponse(EmptyConfirmationPayload)
            ]);

            RecordingSecretRedactor redactor = new();
            SteamConfirmationClient client = BuildClient(handler, redactor);

            client.GetConfirmations(BuildContext());

            Assert.That(redactor.SanitisedAddresses, Has.Some.Contains("k="));
            Assert.That(redactor.SanitisedAddresses, Has.None.Contains(IdentitySecret));
        }

        private static ConfirmationRequestContext BuildContext() => new()
        {
            DeviceIdentifier = "186afecb275bf6db9bb855caa9c916dc56b2dc6a",
            IdentitySecret = IdentitySecret,
            Session = new Domain.Steam.SteamSession
            {
                SteamIdentifier = "76561198028572203",
                AccessToken = "access-token-value"
            }
        };

        private static SteamConfirmationClient BuildClient(RecordingHttpMessageHandler handler)
            => BuildClient(handler, new SecretRedactor());

        private static SteamConfirmationClient BuildClient(
            RecordingHttpMessageHandler handler,
            ISecretRedactor redactor) => new(
            new HttpClient(handler),
            redactor,
            new SteamResponseParser(),
            new ConfirmationRequestBuilder(new ConfirmationRequestSigner()),
            new NullLogger(),
            new SteamGuardSettings());
    }
}