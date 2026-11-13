using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NuciLog.Core;

using SteamGuardDesktop.Configuration;
using SteamGuardDesktop.Domain.Login;
using SteamGuardDesktop.Logging;
using SteamGuardDesktop.Service.Diagnostics;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Communicates with the Steam authentication and two-factor endpoints over HTTPS.
    /// </summary>
    public sealed class SteamAuthenticationClient : ISteamAuthenticationClient
    {
        private static string AuthenticationService => "IAuthenticationService";

        private static string TwoFactorService => "ITwoFactorService";

        private static string GetPasswordRsaKeyMethod => "GetPasswordRSAPublicKey";

        private static string BeginCredentialSessionMethod => "BeginAuthSessionViaCredentials";

        private static string UpdateGuardCodeMethod => "UpdateAuthSessionWithSteamGuardCode";

        private static string PollSessionMethod => "PollAuthSessionStatus";

        private static string QueryStatusMethod => "QueryStatus";

        private static string AddAuthenticatorMethod => "AddAuthenticator";

        private static string FinalizeAuthenticatorMethod => "FinalizeAddAuthenticator";

        private static string RemoveAuthenticatorMethod => "RemoveAuthenticator";

        private static string StartRevocationMethod => "RemoveAuthenticatorViaChallengeStart";

        private static string ContinueRevocationMethod => "RemoveAuthenticatorViaChallengeContinue";

        private static string RemoveAllGuardCookiesPath => "login/remallsteamguardcookies";

        private static int SuccessfulResult => 1;

        private static int MaximumPollingAttempts => 60;

        private static int SuccessfulAuthenticatorStatus => 1;

        private readonly HttpClient httpClient;

        private readonly ISteamResponseParser responseParser;

        private readonly ISteamRequestPayloadBuilder payloadBuilder;

        private readonly ISecretRedactor secretRedactor;

        private readonly ILogger logger;

        private readonly SteamGuardSettings settings;

        /// <summary>
        /// Initialises a new instance of the <see cref="SteamAuthenticationClient"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client used to reach Steam.</param>
        /// <param name="responseParser">The parser used to read Steam responses.</param>
        /// <param name="payloadBuilder">The builder used to create request payloads.</param>
        /// <param name="secretRedactor">The redactor used to keep diagnostics free of secrets.</param>
        /// <param name="logger">The diagnostics logger.</param>
        /// <param name="settings">The application settings.</param>
        public SteamAuthenticationClient(
            HttpClient httpClient,
            ISteamResponseParser responseParser,
            ISteamRequestPayloadBuilder payloadBuilder,
            ISecretRedactor secretRedactor,
            ILogger logger,
            SteamGuardSettings settings)
        {
            ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
            ArgumentNullException.ThrowIfNull(responseParser, nameof(responseParser));
            ArgumentNullException.ThrowIfNull(payloadBuilder, nameof(payloadBuilder));
            ArgumentNullException.ThrowIfNull(secretRedactor, nameof(secretRedactor));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

            this.httpClient = httpClient;
            this.responseParser = responseParser;
            this.payloadBuilder = payloadBuilder;
            this.secretRedactor = secretRedactor;
            this.logger = logger;
            this.settings = settings;
        }

        /// <inheritdoc />
        public SteamClientResult<LoginRsaKey> GetPasswordRsaKey(string accountName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accountName, nameof(accountName));

            string payload = payloadBuilder.BuildPasswordRsaKeyPayload(accountName);

            SteamClientResult<SteamUnifiedResponsePayload> result = SendRequest("GET", AuthenticationService, GetPasswordRsaKeyMethod, payload, string.Empty);

            if (!result.IsSuccessful || result.Value is null)
            {
                return SteamClientResult<LoginRsaKey>.Failed(result.FailureReason);
            }

            SteamUnifiedResponsePayload responsePayload = result.Value;

            if (string.IsNullOrWhiteSpace(responsePayload.PublicKeyModulus) ||
                string.IsNullOrWhiteSpace(responsePayload.PublicKeyExponent))
            {
                return SteamClientResult<LoginRsaKey>.Failed("Steam did not return a password key.");
            }

            return SteamClientResult<LoginRsaKey>.Successful(new LoginRsaKey
            {
                Modulus = responsePayload.PublicKeyModulus,
                Exponent = responsePayload.PublicKeyExponent,
                Timestamp = responsePayload.RsaTimestamp ?? 0
            });
        }

        /// <inheritdoc />
        public SteamClientResult<LoginChallenge> BeginCredentialSession(
            LoginCredentials credentials,
            string encryptedPassword,
            long encryptionTimestamp)
        {
            ArgumentNullException.ThrowIfNull(credentials, nameof(credentials));
            ArgumentException.ThrowIfNullOrWhiteSpace(encryptedPassword, nameof(encryptedPassword));

            string payload = payloadBuilder.BuildBeginCredentialSessionPayload(
                credentials.AccountName,
                encryptedPassword,
                encryptionTimestamp,
                credentials.DeviceFriendlyName);

            SteamClientResult<SteamUnifiedResponsePayload> result = SendRequest("POST", AuthenticationService, BeginCredentialSessionMethod, payload, string.Empty);

            if (!result.IsSuccessful || result.Value is null)
            {
                return SteamClientResult<LoginChallenge>.Failed(result.FailureReason);
            }

            SteamUnifiedResponsePayload responsePayload = result.Value;

            if (string.IsNullOrWhiteSpace(responsePayload.ClientId))
            {
                string errorMessage = responsePayload.ExtendedErrorMessage ?? "Steam rejected the credentials.";

                return SteamClientResult<LoginChallenge>.Failed(errorMessage);
            }

            List<LoginGuardType> acceptedGuards = [.. responsePayload.AllowedConfirmations.Select(guard => MapGuardType(guard.ConfirmationType))];

            string guardMessage = responsePayload.AllowedConfirmations
                .Select(guard => guard.AssociatedMessage)
                .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message)) ?? string.Empty;

            logger.Info(
                SteamGuardOperation.BeginLogin,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.LoginGuard, string.Join(",", acceptedGuards)));

            return SteamClientResult<LoginChallenge>.Successful(new LoginChallenge
            {
                ClientId = responsePayload.ClientId,
                RequestId = responsePayload.RequestId ?? string.Empty,
                PollingIntervalSeconds = responsePayload.PollingIntervalSeconds ?? 5,
                SteamIdentifier = responsePayload.SteamIdentifier ?? string.Empty,
                AcceptedGuards = acceptedGuards,
                GuardMessage = guardMessage
            });
        }

        /// <inheritdoc />
        public SteamClientResult<bool> SubmitGuardCode(
            LoginChallenge challenge,
            string guardCode,
            LoginGuardType guardType)
        {
            ArgumentNullException.ThrowIfNull(challenge, nameof(challenge));
            ArgumentException.ThrowIfNullOrWhiteSpace(guardCode, nameof(guardCode));

            string payload = payloadBuilder.BuildGuardCodePayload(
                challenge.ClientId,
                challenge.SteamIdentifier,
                guardCode,
                (int)guardType);

            SteamClientResult<SteamUnifiedResponsePayload> result = SendRequest("POST", AuthenticationService, UpdateGuardCodeMethod, payload, string.Empty);

            if (!result.IsSuccessful)
            {
                return SteamClientResult<bool>.Failed(result.FailureReason);
            }

            logger.Info(
                SteamGuardOperation.SubmitLoginGuard,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.LoginGuard, guardType.ToString()));

            return SteamClientResult<bool>.Successful(true);
        }

        /// <inheritdoc />
        public SteamClientResult<LoginTokens> PollSession(LoginChallenge challenge)
        {
            ArgumentNullException.ThrowIfNull(challenge, nameof(challenge));

            string payload = payloadBuilder.BuildPollSessionPayload(challenge.ClientId, challenge.RequestId);
            int pollingDelayMilliseconds = (int)Math.Ceiling(challenge.PollingIntervalSeconds * 1000);

            for (int attempt = 0; attempt < MaximumPollingAttempts; ++attempt)
            {
                SteamClientResult<SteamUnifiedResponsePayload> result = SendRequest("POST", AuthenticationService, PollSessionMethod, payload, string.Empty);

                if (!result.IsSuccessful || result.Value is null)
                {
                    return SteamClientResult<LoginTokens>.Failed(result.FailureReason);
                }

                SteamUnifiedResponsePayload responsePayload = result.Value;

                if (!string.IsNullOrWhiteSpace(responsePayload.AccessToken) ||
                    !string.IsNullOrWhiteSpace(responsePayload.RefreshToken))
                {
                    logger.Info(
                        SteamGuardOperation.PollLoginSession,
                        OperationStatus.Success,
                        new LogInfo(SteamGuardInfoKey.AccountIdentifier, challenge.SteamIdentifier));

                    return SteamClientResult<LoginTokens>.Successful(new LoginTokens
                    {
                        SteamIdentifier = responsePayload.SteamIdentifier ?? challenge.SteamIdentifier,
                        AccessToken = responsePayload.AccessToken ?? string.Empty,
                        RefreshToken = responsePayload.RefreshToken ?? string.Empty
                    });
                }

                Thread.Sleep(pollingDelayMilliseconds);
            }

            return SteamClientResult<LoginTokens>.Failed("Steam did not finish the sign-in in time.");
        }

        /// <inheritdoc />
        public SteamClientResult<AuthenticatorStatus> GetAuthenticatorStatus(
            string accessToken,
            string steamIdentifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(steamIdentifier, nameof(steamIdentifier));

            string payload = ComposeJson(("steamid", steamIdentifier));

            SteamClientResult<SteamUnifiedResponsePayload> result = SendRequest(
                "POST",
                TwoFactorService,
                QueryStatusMethod,
                payload,
                accessToken);

            if (!result.IsSuccessful || result.Value is null)
            {
                return SteamClientResult<AuthenticatorStatus>.Failed(result.FailureReason);
            }

            SteamUnifiedResponsePayload responsePayload = result.Value;

            return SteamClientResult<AuthenticatorStatus>.Successful(new AuthenticatorStatus
            {
                HasMobileAuthenticator = responsePayload.AuthenticatorType == SuccessfulAuthenticatorStatus,
                AuthenticatorType = responsePayload.AuthenticatorType ?? 0,
                SteamGuardScheme = responsePayload.SteamGuardScheme ?? 0,
                DeviceIdentifier = responsePayload.DeviceIdentifier ?? string.Empty,
                RevocationAttemptsRemaining = responsePayload.RevocationAttemptsRemaining ?? 0,
                IsAuthenticatorAllowed = responsePayload.IsAuthenticatorAllowed ?? false
            });
        }

        /// <inheritdoc />
        public SteamClientResult<bool> RemoveAuthenticator(
            string accessToken,
            string steamIdentifier,
            string revocationCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(steamIdentifier, nameof(steamIdentifier));
            ArgumentException.ThrowIfNullOrWhiteSpace(revocationCode, nameof(revocationCode));

            string payload = ComposeJson(
                ("steamid", steamIdentifier),
                ("revocation_code", revocationCode),
                ("revocation_reason", "4"),
                ("steamguard_scheme", "2"));

            SteamClientResult<SteamUnifiedResponsePayload> result = SendRequest(
                "POST",
                TwoFactorService,
                RemoveAuthenticatorMethod,
                payload,
                accessToken);

            if (!result.IsSuccessful || result.Value is null)
            {
                return SteamClientResult<bool>.Failed(result.FailureReason);
            }

            if (result.Value.IsSuccessful != true)
            {
                return SteamClientResult<bool>.Failed("Steam rejected the revocation code.");
            }

            return SteamClientResult<bool>.Successful(true);
        }

        /// <inheritdoc />
        public SteamClientResult<EnrolledAuthenticator> AddAuthenticator(
            string accessToken,
            string steamIdentifier,
            string deviceIdentifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(steamIdentifier, nameof(steamIdentifier));
            ArgumentException.ThrowIfNullOrWhiteSpace(deviceIdentifier, nameof(deviceIdentifier));

            string payload = ComposeJson(
                ("steamid", steamIdentifier),
                ("authenticator_type", "1"),
                ("device_identifier", deviceIdentifier),
                ("sms_phone_id", "1"),
                ("version", "1"));

            SteamClientResult<SteamUnifiedResponsePayload> result = SendRequest(
                "POST",
                TwoFactorService,
                AddAuthenticatorMethod,
                payload,
                accessToken);

            if (!result.IsSuccessful || result.Value is null)
            {
                return SteamClientResult<EnrolledAuthenticator>.Failed(result.FailureReason);
            }

            SteamUnifiedResponsePayload responsePayload = result.Value;

            if (responsePayload.SharedSecret is null || responsePayload.IdentitySecret is null)
            {
                return SteamClientResult<EnrolledAuthenticator>.Failed(
                    "Steam did not return the authenticator secrets.");
            }

            logger.Info(
                SteamGuardOperation.AddAuthenticator,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.AccountIdentifier, steamIdentifier));

            return SteamClientResult<EnrolledAuthenticator>.Successful(new EnrolledAuthenticator
            {
                SteamIdentifier = steamIdentifier,
                SharedSecret = SteamSharedSecretEncoder.Encode(responsePayload.SharedSecret),
                IdentitySecret = Convert.ToBase64String(responsePayload.IdentitySecret),
                RevocationCode = responsePayload.RevocationCode ?? string.Empty,
                DeviceIdentifier = responsePayload.DeviceIdentifier ?? deviceIdentifier
            });
        }

        /// <inheritdoc />
        public SteamClientResult<AuthenticatorFinalizationState> FinalizeAuthenticator(
            string accessToken,
            string steamIdentifier,
            string activationCode,
            string deviceIdentifier,
            string smsActivationCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(steamIdentifier, nameof(steamIdentifier));
            ArgumentException.ThrowIfNullOrWhiteSpace(activationCode, nameof(activationCode));
            ArgumentException.ThrowIfNullOrWhiteSpace(smsActivationCode, nameof(smsActivationCode));

            string payload = ComposeJson(
                ("steamid", steamIdentifier),
                ("authenticator_code", activationCode),
                ("authenticator_time", ComposeUnixTime().ToString(CultureInfo.InvariantCulture)),
                ("device_identifier", deviceIdentifier),
                ("activation_code", smsActivationCode),
                ("validate_sms_code", "1"));

            SteamClientResult<SteamUnifiedResponsePayload> result = SendRequest(
                "POST",
                TwoFactorService,
                FinalizeAuthenticatorMethod,
                payload,
                accessToken);

            if (!result.IsSuccessful || result.Value is null)
            {
                return SteamClientResult<AuthenticatorFinalizationState>.Failed(result.FailureReason);
            }

            if (result.Value.IsSuccessful == true)
            {
                return SteamClientResult<AuthenticatorFinalizationState>.Successful(
                    AuthenticatorFinalizationState.Succeeded);
            }

            if (result.Value.WantsMore == true)
            {
                return SteamClientResult<AuthenticatorFinalizationState>.Successful(
                    AuthenticatorFinalizationState.WantsMore);
            }

            return SteamClientResult<AuthenticatorFinalizationState>.Failed("Steam rejected the activation code.");
        }

        /// <inheritdoc />
        public SteamClientResult<bool> StartAuthenticatorRevocation(string steamIdentifier, string accessToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(steamIdentifier, nameof(steamIdentifier));

            string payload = ComposeJson(("steamid", steamIdentifier));

            SteamClientResult<SteamUnifiedResponsePayload> result = SendRequest(
                "POST",
                TwoFactorService,
                StartRevocationMethod,
                payload,
                accessToken);

            if (!result.IsSuccessful || result.Value is null)
            {
                return SteamClientResult<bool>.Failed(result.FailureReason);
            }

            if (result.Value.IsSuccessful != true)
            {
                return SteamClientResult<bool>.Failed("Steam rejected the revocation request.");
            }

            return SteamClientResult<bool>.Successful(true);
        }

        /// <inheritdoc />
        public SteamClientResult<EnrolledAuthenticator> ContinueAuthenticatorRevocation(
            string accessToken,
            string smsCode,
            string steamIdentifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(smsCode, nameof(smsCode));
            ArgumentException.ThrowIfNullOrWhiteSpace(steamIdentifier, nameof(steamIdentifier));

            string payload = ComposeJson(
                ("sms_code", smsCode),
                ("generate_new_token", "1"),
                ("version", "1"));

            SteamClientResult<SteamUnifiedResponsePayload> result = SendRequest(
                "POST",
                TwoFactorService,
                ContinueRevocationMethod,
                payload,
                accessToken);

            if (!result.IsSuccessful || result.Value is null)
            {
                return SteamClientResult<EnrolledAuthenticator>.Failed(result.FailureReason);
            }

            SteamUnifiedResponsePayload responsePayload = result.Value;

            if (responsePayload.IsSuccessful != true ||
                responsePayload.SharedSecret is null ||
                responsePayload.IdentitySecret is null)
            {
                return SteamClientResult<EnrolledAuthenticator>.Failed("Steam rejected the recovery code.");
            }

            logger.Info(
                SteamGuardOperation.RecoverAuthenticator,
                OperationStatus.Success,
                new LogInfo(SteamGuardInfoKey.AccountIdentifier, steamIdentifier));

            return SteamClientResult<EnrolledAuthenticator>.Successful(new EnrolledAuthenticator
            {
                SteamIdentifier = steamIdentifier,
                SharedSecret = SteamSharedSecretEncoder.Encode(responsePayload.SharedSecret),
                IdentitySecret = Convert.ToBase64String(responsePayload.IdentitySecret),
                RevocationCode = responsePayload.RevocationCode ?? string.Empty
            });
        }

        /// <inheritdoc />
        public SteamClientResult<bool> RemoveAllGuardCookies(string accessToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accessToken, nameof(accessToken));

            string requestAddress = CombineAddress(settings.SteamCommunityBaseAddress, RemoveAllGuardCookiesPath);

            using HttpRequestMessage request = new(HttpMethod.Get, requestAddress);
            request.Headers.Add("Cookie", "steamLoginSecure=" + accessToken);

            try
            {
                using HttpResponseMessage response = httpClient.Send(request);

                if (!response.IsSuccessStatusCode)
                {
                    return SteamClientResult<bool>.Failed(DescribeStatus(response.StatusCode));
                }

                return SteamClientResult<bool>.Successful(true);
            }
            catch (HttpRequestException exception)
            {
                logger.Error(SteamGuardOperation.RemoveAuthenticator, exception, new LogInfo(SteamGuardInfoKey.FailureReason, exception.Message));

                return SteamClientResult<bool>.Failed("Steam could not be reached.");
            }
        }

        private static string ComposeJson(params (string Name, string Value)[] fields)
        {
            StringBuilder builder = new("{");

            for (int fieldIndex = 0; fieldIndex < fields.Length; ++fieldIndex)
            {
                if (fieldIndex > 0)
                {
                    builder.Append(',');
                }

                builder
                    .Append('"')
                    .Append(fields[fieldIndex].Name)
                    .Append("\":\"")
                    .Append(fields[fieldIndex].Value)
                    .Append('"');
            }

            return builder.Append('}').ToString();
        }

        private static long ComposeUnixTime() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

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

        private static LoginGuardType MapGuardType(int reportedType)
        {
            if (Enum.IsDefined(typeof(LoginGuardType), reportedType))
            {
                return (LoginGuardType)reportedType;
            }

            return LoginGuardType.Unsupported;
        }

        private string ComposeRequestAddress(string service, string method)
            => CombineAddress(settings.SteamApiBaseAddress, $"{service}/{method}/v1");

        private static string CombineAddress(string baseAddress, string relativePath)
            => new Uri(new Uri(baseAddress, UriKind.Absolute), relativePath).AbsoluteUri;

        private SteamClientResult<SteamUnifiedResponsePayload> SendRequest(
            string httpMethod,
            string service,
            string method,
            string payload,
            string accessToken)
        {
            string requestAddress = ComposeRequestAddress(service, method);
            string encodedPayload = WebUtility.UrlEncode(payload);

            using HttpRequestMessage request = new(new HttpMethod(httpMethod), requestAddress);

            if (httpMethod == "GET")
            {
                request.RequestUri = new Uri(requestAddress + "?input_json=" + encodedPayload);
            }
            else
            {
                request.Content = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("input_json", payload)
                ]);
            }

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            logger.Debug(
                SteamGuardOperation.SendSteamRequest,
                OperationStatus.Started,
                new LogInfo(SteamGuardInfoKey.RequestAddress, secretRedactor.SanitiseAddress(requestAddress)),
                new LogInfo(SteamGuardInfoKey.SteamOperation, method));

            try
            {
                using HttpResponseMessage response = httpClient.Send(request);

                if (!response.IsSuccessStatusCode)
                {
                    return SteamClientResult<SteamUnifiedResponsePayload>.Failed(DescribeStatus(response.StatusCode));
                }

                string? responseBody = BoundedResponseReader.ReadAsString(response, settings.MaximumResponseBytes);

                if (responseBody is null)
                {
                    return SteamClientResult<SteamUnifiedResponsePayload>.Failed("The Steam response exceeded the accepted size.");
                }

                if (!responseParser.TryParse(responseBody, out SteamUnifiedResponse? unifiedResponse) ||
                    unifiedResponse is null)
                {
                    return SteamClientResult<SteamUnifiedResponsePayload>.Failed("The Steam response could not be read.");
                }

                if (unifiedResponse.Result != SuccessfulResult && unifiedResponse.Result != 0)
                {
                    return SteamClientResult<SteamUnifiedResponsePayload>.Failed(
                        $"Steam returned the result code {unifiedResponse.Result}.");
                }

                return SteamClientResult<SteamUnifiedResponsePayload>.Successful(
                    unifiedResponse.Payload ?? new SteamUnifiedResponsePayload());
            }
            catch (HttpRequestException exception)
            {
                logger.Error(SteamGuardOperation.SendSteamRequest, exception, new LogInfo(SteamGuardInfoKey.FailureReason, exception.Message));

                return SteamClientResult<SteamUnifiedResponsePayload>.Failed("Steam could not be reached.");
            }
            catch (TaskCanceledException exception)
            {
                logger.Error(SteamGuardOperation.SendSteamRequest, exception, new LogInfo(SteamGuardInfoKey.FailureReason, exception.Message));

                return SteamClientResult<SteamUnifiedResponsePayload>.Failed("The request to Steam timed out.");
            }
        }
    }
}
