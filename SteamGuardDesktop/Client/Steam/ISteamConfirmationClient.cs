using System.Collections.Generic;

using SteamGuardDesktop.Domain.Confirmations;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Communicates with the Steam confirmation and time endpoints.
    /// </summary>
    public interface ISteamConfirmationClient
    {
        /// <summary>
        /// Retrieves the current time reported by the Steam servers.
        /// </summary>
        /// <returns>The Steam server time expressed as a Unix timestamp in seconds.</returns>
        SteamClientResult<long> GetServerTime();

        /// <summary>
        /// Retrieves the pending confirmations of the account.
        /// </summary>
        /// <param name="context">The account material required to sign the request.</param>
        /// <returns>The pending confirmations.</returns>
        SteamClientResult<IEnumerable<Confirmation>> GetConfirmations(ConfirmationRequestContext context);

        /// <summary>
        /// Retrieves the safe descriptive text of a pending confirmation.
        /// </summary>
        /// <param name="context">The account material required to sign the request.</param>
        /// <param name="confirmationIdentifier">The identifier of the confirmation to inspect.</param>
        /// <returns>The descriptive text of the confirmation.</returns>
        SteamClientResult<string> GetConfirmationDetails(ConfirmationRequestContext context, string confirmationIdentifier);

        /// <summary>
        /// Applies a decision to a pending confirmation.
        /// </summary>
        /// <param name="context">The account material required to sign the request.</param>
        /// <param name="decision">The decision to apply.</param>
        /// <returns>The outcome of the decision.</returns>
        SteamClientResult<bool> ApplyDecision(ConfirmationRequestContext context, ConfirmationDecision decision);
    }
}
