using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

using SteamGuardDesktop.Domain.Confirmations;
using SteamGuardDesktop.Service.Confirmations;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Builds the signed query parameters required by the Steam confirmation endpoints.
    /// </summary>
    public sealed class ConfirmationRequestBuilder(IConfirmationRequestSigner signer)
    {
        private static string ListTag => "conf";

        /// <summary>
        /// Builds the signed query parameters for a request signed at the specified Steam time.
        /// </summary>
        /// <param name="context">The account material required to sign the request.</param>
        /// <param name="tag">The request tag expected by Steam.</param>
        /// <param name="unixTimeSeconds">The Steam server time expressed as a Unix timestamp in seconds.</param>
        /// <returns>The ordered query parameters.</returns>
        public IEnumerable<KeyValuePair<string, string>> BuildSignedParameters(
            ConfirmationRequestContext context,
            string tag,
            long unixTimeSeconds)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tag));

            string signature = signer.GenerateSignature(context.IdentitySecret, unixTimeSeconds, tag);

            return
            [
                new("p", context.DeviceIdentifier),
                new("a", context.Session.SteamIdentifier),
                new("k", signature),
                new("t", unixTimeSeconds.ToString(CultureInfo.InvariantCulture)),
                new("m", "react"),
                new("tag", tag)
            ];
        }

        /// <summary>
        /// Builds the encoded query string of a confirmation list request.
        /// </summary>
        /// <param name="context">The account material required to sign the request.</param>
        /// <param name="unixTimeSeconds">The Steam server time expressed as a Unix timestamp in seconds.</param>
        /// <returns>The encoded query string.</returns>
        public string BuildConfirmationListQuery(ConfirmationRequestContext context, long unixTimeSeconds)
            => Encode(BuildSignedParameters(context, ListTag, unixTimeSeconds));

        /// <summary>
        /// Builds the encoded query string of a confirmation decision request.
        /// </summary>
        /// <param name="context">The account material required to sign the request.</param>
        /// <param name="decision">The decision to apply.</param>
        /// <param name="unixTimeSeconds">The Steam server time expressed as a Unix timestamp in seconds.</param>
        /// <returns>The encoded query string.</returns>
        public string BuildDecisionQuery(
            ConfirmationRequestContext context,
            ConfirmationDecision decision,
            long unixTimeSeconds)
        {
            ArgumentNullException.ThrowIfNull(decision, nameof(decision));

            List<KeyValuePair<string, string>> parameters =
            [
                .. BuildSignedParameters(context, ListTag, unixTimeSeconds)
            ];

            parameters.Add(new("op", decision.OperationName));
            parameters.Add(new("cid", decision.ConfirmationIdentifier));
            parameters.Add(new("ck", decision.Nonce));

            return Encode(parameters);
        }

        private static string Encode(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            StringBuilder builder = new();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                if (builder.Length > 0)
                {
                    builder.Append('&');
                }

                builder
                    .Append(WebUtility.UrlEncode(parameter.Key))
                    .Append('=')
                    .Append(WebUtility.UrlEncode(parameter.Value));
            }

            return builder.ToString();
        }
    }
}
