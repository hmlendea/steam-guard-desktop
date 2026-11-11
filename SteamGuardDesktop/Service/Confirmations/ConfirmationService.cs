using System;
using System.Collections.Generic;

using SteamGuardDesktop.Client.Steam;
using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Domain.Authenticator;
using SteamGuardDesktop.Domain.Confirmations;
using SteamGuardDesktop.Domain.Steam;
using SteamGuardDesktop.Service.Account;

namespace SteamGuardDesktop.Service.Confirmations
{
    /// <summary>
    /// Coordinates the pending confirmations of an account.
    /// </summary>
    public sealed class ConfirmationService(
        ISteamConfirmationClient steamClient,
        IAccountService accountService) : IConfirmationService
    {
        /// <inheritdoc />
        public ConfirmationListResult GetConfirmations(SteamAccount account)
        {
            ArgumentNullException.ThrowIfNull(account, nameof(account));

            ConfirmationRequestContext? context = BuildContext(account);

            if (context is null)
            {
                return ConfirmationListResult.Failed("The account secrets are unavailable.", false);
            }

            SteamClientResult<IEnumerable<Confirmation>> result = steamClient.GetConfirmations(context);

            if (!result.IsSuccessful)
            {
                return ConfirmationListResult.Failed(result.FailureReason, result.RequiresAuthentication);
            }

            return ConfirmationListResult.Successful(result.Value ?? []);
        }

        /// <inheritdoc />
        public ConfirmationDecisionResult ApplyDecision(SteamAccount account, ConfirmationDecision decision)
        {
            ArgumentNullException.ThrowIfNull(account, nameof(account));
            ArgumentNullException.ThrowIfNull(decision, nameof(decision));

            ConfirmationRequestContext? context = BuildContext(account);

            if (context is null)
            {
                return ConfirmationDecisionResult.Failed("The account secrets are unavailable.", false);
            }

            SteamClientResult<bool> result = steamClient.ApplyDecision(context, decision);

            if (!result.IsSuccessful)
            {
                return ConfirmationDecisionResult.Failed(result.FailureReason, result.RequiresAuthentication);
            }

            return ConfirmationDecisionResult.Successful();
        }

        private ConfirmationRequestContext? BuildContext(SteamAccount account)
        {
            AuthenticatorSecrets? secrets = accountService.GetSecrets(account);

            if (secrets is null || !secrets.AreSecretsPresent)
            {
                return null;
            }

            return new ConfirmationRequestContext
            {
                DeviceIdentifier = account.DeviceIdentifier,
                IdentitySecret = secrets.IdentitySecret,
                Session = new SteamSession
                {
                    SteamIdentifier = account.SteamIdentifier,
                    AccessToken = secrets.AccessToken
                }
            };
        }
    }
}
