using System;
using System.Collections.Generic;

using NUnit.Framework;

using SteamGuardDesktop.Client.Steam;
using SteamGuardDesktop.IntegrationTests.Utils;
using SteamGuardDesktop.Service.Confirmations;

namespace SteamGuardDesktop.IntegrationTests.Steam
{
    [TestFixture]
    public sealed class SteamConfirmationDetailsTests
    {
        private static string IdentitySecret => "GQP46b73Ws7gr8GmZFR0sDuau5c=";

        private static string ServerTimePayload => "{\"response\":{\"server_time\":\"1617591917\"}}";

        [Test]
        public void GivenAConfirmationDetailsResponse_WhenRequestingTheDetails_ThenTheDetailsPathAndTagAreUsed()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(ServerTimePayload),
                RecordingHttpMessageHandler.CreateJsonResponse("{\"success\":true,\"html\":\"<div>Trade offer</div>\"}")
            ]);

            SteamConfirmationClient client = BuildClient(handler);

            SteamClientResult<string> result = client.GetConfirmationDetails(BuildContext(), "613");

            Assert.That(result.IsSuccessful);
            Assert.That(result.Value, Is.EqualTo("Trade offer"));
            Assert.That(handler.Requests[1].RequestUri?.AbsolutePath, Is.EqualTo("/mobileconf/details/613"));
            Assert.That(handler.Requests[1].RequestUri?.Query, Does.Contain("tag=details"));
        }

        [Test]
        public void GivenARejectedDetailsRequest_WhenRequestingTheDetails_ThenTheRequestFails()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(ServerTimePayload),
                RecordingHttpMessageHandler.CreateJsonResponse("{\"success\":false}")
            ]);

            SteamConfirmationClient client = BuildClient(handler);

            SteamClientResult<string> result = client.GetConfirmationDetails(BuildContext(), "613");

            Assert.That(result.IsSuccessful, Is.False);
        }

        [Test]
        public void GivenMalformedDetailsJson_WhenRequestingTheDetails_ThenTheRequestFails()
        {
            RecordingHttpMessageHandler handler = new(
            [
                RecordingHttpMessageHandler.CreateJsonResponse(ServerTimePayload),
                RecordingHttpMessageHandler.CreateJsonResponse("{malformed")
            ]);

            SteamConfirmationClient client = BuildClient(handler);

            SteamClientResult<string> result = client.GetConfirmationDetails(BuildContext(), "613");

            Assert.That(result.IsSuccessful, Is.False);
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

        private static SteamConfirmationClient BuildClient(RecordingHttpMessageHandler handler) => new(
            new System.Net.Http.HttpClient(handler),
            new Service.Diagnostics.SecretRedactor(),
            new SteamResponseParser(),
            new ConfirmationRequestBuilder(new ConfirmationRequestSigner()),
            new NuciLog.Core.NullLogger(),
            new Configuration.SteamGuardSettings());
    }
}
