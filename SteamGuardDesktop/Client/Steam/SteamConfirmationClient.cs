using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using NuciLog.Core;

using SteamGuardDesktop.Configuration;
using SteamGuardDesktop.Domain.Confirmations;
using SteamGuardDesktop.Logging;
using SteamGuardDesktop.Service.Diagnostics;
using SteamGuardDesktop.Service.Mapping;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Communicates with the Steam confirmation and time endpoints over HTTPS.
    /// </summary>
    public sealed class SteamConfirmationClient : ISteamConfirmationClient
    {
        private static string ConirmationListPath => "mobileconf/getlist";

        private static string ConfirmationOperationPath => "mobileconf/ajaxop";

        private static string ConfirmationDetailsPathPrefix => "mobileconf/details/";

        private static string ServerTimePath => "ITwoFactorService/QueryTime/v0001";

        private static string SteamCookieDomain => "https://steamcommunity.com";

        private static string SessionCookieName => "steamLoginSecure";

        private static string SessionCookieSeparator => "||";

        private static string SteamIdentifierCookieName => "steamid";

        private static string AcceptLanguageHeaderValue => "en-US,en;q=0.9";

        private static string ContentTypeHeaderValue => "application/json";

        private readonly HttpClient httpClient;

        private readonly ISecretRedactor secretRedactor;

        private readonly ISteamResponseParser responseParser;

        private readonly ConfirmationRequestBuilder requestBuilder;

        private readonly ILogger logger;

        private readonly SteamGuardSettings settings;

        /// <summary>
        /// Initialises a new instance of the <see cref="SteamConfirmationClient"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client used to reach Steam.</param>
        /// <param name="secretRedactor">The redactor used to keep diagnostics free of secrets.</param>
        /// <param name="responseParser">The parser used to read Steam responses.</param>
        /// <param name="requestBuilder">The builder used to sign confirmation requests.</param>
        /// <param name="logger">The diagnostics logger.</param>
        /// <param name="settings">The application settings.</param>
        public SteamConfirmationClient(
            HttpClient httpClient,
            ISecretRedactor secretRedactor,
            ISteamResponseParser responseParser,
            ConfirmationRequestBuilder requestBuilder,
            ILogger logger,
            SteamGuardSettings settings)
        {
            ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
            ArgumentNullException.ThrowIfNull(secretRedactor, nameof(secretRedactor));
            ArgumentNullException.ThrowIfNull(responseParser, nameof(responseParser));
            ArgumentNullException.ThrowIfNull(requestBuilder, nameof(requestBuilder));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

            this.httpClient = httpClient;
            this.secretRedactor = secretRedactor;
            this.responseParser = responseParser;
            this.requestBuilder = requestBuilder;
            this.logger = logger;
            this.settings = settings;
        }

        /// <inheritdoc />
        public SteamClientResult<long> GetServerTime()
        {
            string requestAddress = CombineAddress(settings.SteamApiBaseAddress, ServerTimePath);

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(requestAddress).GetAwaiter().GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    return SteamClientResult<long>.Failed(DescribeStatus(response.StatusCode));
                }

                string? payload = BoundedResponseReader.ReadAsString(response, settings.MaximumResponseBytes);

                if (payload is null)
                {
                    return SteamClientResult<long>.Failed("The Steam response exceeded the accepted size.");
                }

                if (!responseParser.TryParse(payload, out GetServerTimeResponse? serverTimeResponse) ||
                    serverTimeResponse?.Response is null ||
                    !long.TryParse(
                        serverTimeResponse.Response.ServerTime,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out long serverTime))
                {
                    return SteamClientResult<long>.Failed("The Steam server time response could not be read.");
                }

                return SteamClientResult<long>.Successful(serverTime);
            }
            catch (HttpRequestException exception)
            {
                logger.Error(SteamGuardOperation.SendSteamRequest, exception, new LogInfo(SteamGuardInfoKey.FailureReason, exception.Message));

                return SteamClientResult<long>.Failed("Steam could not be reached.");
            }
            catch (TaskCanceledException exception)
            {
                logger.Error(SteamGuardOperation.SendSteamRequest, exception, new LogInfo(SteamGuardInfoKey.FailureReason, exception.Message));

                return SteamClientResult<long>.Failed("The request to Steam timed out.");
            }
        }

        /// <inheritdoc />
        public SteamClientResult<IEnumerable<Confirmation>> GetConfirmations(ConfirmationRequestContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            SteamClientResult<long> serverTimeResult = GetServerTime();

            if (!serverTimeResult.IsSuccessful)
            {
                return SteamClientResult<IEnumerable<Confirmation>>.Failed(serverTimeResult.FailureReason);
            }

            string query = requestBuilder.BuildConfirmationListQuery(context, serverTimeResult.Value);
            string requestAddress = CombineAddress(settings.SteamCommunityBaseAddress, ConirmationListPath) + "?" + query;

            SteamClientResult<string> payloadResult = SendSignedRequest(
                SteamGuardOperation.LoadConfirmations,
                requestAddress,
                context);

            if (!payloadResult.IsSuccessful || payloadResult.Value is null)
            {
                return SteamClientResult<IEnumerable<Confirmation>>.Failed(payloadResult.FailureReason);
            }

            if (!responseParser.TryParse(payloadResult.Value, out GetConfirmationListResponse? listResponse) ||
                listResponse is null)
            {
                return SteamClientResult<IEnumerable<Confirmation>>.Failed("The confirmation list could not be read.");
            }

            if (listResponse.RequiresAuthentication == true)
            {
                return SteamClientResult<IEnumerable<Confirmation>>.AuthenticationRequired(
                    "The Steam session is no longer valid.");
            }

            if (!listResponse.IsSuccessful)
            {
                return SteamClientResult<IEnumerable<Confirmation>>.Failed(DescribeRemoteFailure(listResponse.Message));
            }

            List<Confirmation> confirmations = [.. listResponse.Confirmations.Select(dataObject => dataObject.ToServiceModel())];

            logger.Info(
                SteamGuardOperation.LoadConfirmations,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.ConfirmationCount, confirmations.Count));

            return SteamClientResult<IEnumerable<Confirmation>>.Successful(confirmations);
        }

        /// <inheritdoc />
        public SteamClientResult<string> GetConfirmationDetails(
            ConfirmationRequestContext context,
            string confirmationIdentifier)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentException.ThrowIfNullOrWhiteSpace(confirmationIdentifier, nameof(confirmationIdentifier));

            SteamClientResult<long> serverTimeResult = GetServerTime();

            if (!serverTimeResult.IsSuccessful)
            {
                return SteamClientResult<string>.Failed(serverTimeResult.FailureReason);
            }

            string query = requestBuilder.BuildConfirmationDetailsQuery(context, serverTimeResult.Value);
            string requestAddress = CombineAddress(
                settings.SteamCommunityBaseAddress,
                ConfirmationDetailsPathPrefix + Uri.EscapeDataString(confirmationIdentifier)) + "?" + query;

            SteamClientResult<string> payloadResult = SendSignedRequest(
                SteamGuardOperation.LoadConfirmationDetails,
                requestAddress,
                context);

            if (!payloadResult.IsSuccessful || payloadResult.Value is null)
            {
                return SteamClientResult<string>.Failed(payloadResult.FailureReason);
            }

            if (!responseParser.TryParse(payloadResult.Value, out GetConfirmationDetailsResponse? detailsResponse) ||
                detailsResponse is null)
            {
                return SteamClientResult<string>.Failed("The confirmation details could not be read.");
            }

            if (!detailsResponse.IsSuccessful)
            {
                return SteamClientResult<string>.Failed("Steam rejected the details request.");
            }

            return SteamClientResult<string>.Successful(
                ConfirmationDetailsTextConverter.ToPlainText(detailsResponse.Html));
        }

        /// <inheritdoc />
        public SteamClientResult<bool> ApplyDecision(ConfirmationRequestContext context, ConfirmationDecision decision)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(decision, nameof(decision));

            SteamClientResult<long> serverTimeResult = GetServerTime();

            if (!serverTimeResult.IsSuccessful)
            {
                return SteamClientResult<bool>.Failed(serverTimeResult.FailureReason);
            }

            string query = requestBuilder.BuildDecisionQuery(context, decision, serverTimeResult.Value);
            string requestAddress = CombineAddress(settings.SteamCommunityBaseAddress, ConfirmationOperationPath) + "?" + query;

            SteamClientResult<string> payloadResult = SendSignedRequest(
                SteamGuardOperation.ApplyConfirmationDecision,
                requestAddress,
                context);

            if (!payloadResult.IsSuccessful || payloadResult.Value is null)
            {
                return SteamClientResult<bool>.Failed(payloadResult.FailureReason);
            }

            if (!responseParser.TryParse(payloadResult.Value, out SendConfirmationResponse? decisionResponse) ||
                decisionResponse is null)
            {
                return SteamClientResult<bool>.Failed("The decision response could not be read.");
            }

            if (decisionResponse.RequiresAuthentication == true)
            {
                return SteamClientResult<bool>.AuthenticationRequired("The Steam session is no longer valid.");
            }

            if (!decisionResponse.IsSuccessful)
            {
                return SteamClientResult<bool>.Failed(DescribeRemoteFailure(decisionResponse.Message));
            }

            logger.Info(
                SteamGuardOperation.ApplyConfirmationDecision,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.SteamOperation, decision.OperationName));

            return SteamClientResult<bool>.Successful(true);
        }

        private static string CombineAddress(string baseAddress, string relativePath)
            => new Uri(new Uri(baseAddress, UriKind.Absolute), relativePath).AbsoluteUri;

        private static string DescribeStatus(HttpStatusCode statusCode)
            => $"Steam returned the status code {(int)statusCode}.";

        private static string DescribeRemoteFailure(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "Steam rejected the request without providing a reason.";
            }

            return $"Steam rejected the request: {message}";
        }

        private SteamClientResult<string> SendSignedRequest(
            Operation operation,
            string requestAddress,
            ConfirmationRequestContext context)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, requestAddress);
            request.Headers.Add("Cookie", BuildSessionCookie(context));
            request.Headers.Add("Accept-Language", AcceptLanguageHeaderValue);

            logger.Debug(
                operation,
                OperationStatus.Started,
                new LogInfo(SteamGuardInfoKey.RequestAddress, secretRedactor.SanitiseAddress(requestAddress)));

            try
            {
                using HttpResponseMessage response = httpClient.Send(request);

                logger.Debug(
                    operation,
                    OperationStatus.InProgress,
                    new LogInfo(SteamGuardInfoKey.HttpStatusCode, (int)response.StatusCode));

                if (!response.IsSuccessStatusCode)
                {
                    return SteamClientResult<string>.Failed(DescribeStatus(response.StatusCode));
                }

                string? payload = BoundedResponseReader.ReadAsString(response, settings.MaximumResponseBytes);

                if (payload is null)
                {
                    return SteamClientResult<string>.Failed("The Steam response exceeded the accepted size.");
                }

                return SteamClientResult<string>.Successful(payload);
            }
            catch (HttpRequestException exception)
            {
                logger.Error(operation, exception, new LogInfo(SteamGuardInfoKey.FailureReason, exception.Message));

                return SteamClientResult<string>.Failed("Steam could not be reached.");
            }
            catch (TaskCanceledException exception)
            {
                logger.Error(operation, exception, new LogInfo(SteamGuardInfoKey.FailureReason, exception.Message));

                return SteamClientResult<string>.Failed("The request to Steam timed out.");
            }
        }

        private static string BuildSessionCookie(ConfirmationRequestContext context)
            => $"{SessionCookieName}={context.Session.SteamIdentifier}" +
                SessionCookieSeparator +
                context.Session.AccessToken +
                $"; {SteamIdentifierCookieName}={context.Session.SteamIdentifier}";
    }
}
