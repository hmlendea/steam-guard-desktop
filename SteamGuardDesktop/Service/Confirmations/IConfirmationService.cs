using System.Collections.Generic;

using SteamGuardDesktop.Domain.Account;
using SteamGuardDesktop.Domain.Confirmations;

namespace SteamGuardDesktop.Service.Confirmations
{
    /// <summary>
    /// Coordinates the pending confirmations of an account.
    /// </summary>
    public interface IConfirmationService
    {
        /// <summary>
        /// Loads the pending confirmations of an account.
        /// </summary>
        /// <param name="account">The account whose confirmations are requested.</param>
        /// <returns>The outcome of the operation.</returns>
        ConfirmationListResult GetConfirmations(SteamAccount account);

        /// <summary>
        /// Applies a decision to a pending confirmation.
        /// </summary>
        /// <param name="account">The account that owns the confirmation.</param>
        /// <param name="decision">The decision to apply.</param>
        /// <returns>The outcome of the operation.</returns>
        ConfirmationDecisionResult ApplyDecision(SteamAccount account, ConfirmationDecision decision);
    }
}
