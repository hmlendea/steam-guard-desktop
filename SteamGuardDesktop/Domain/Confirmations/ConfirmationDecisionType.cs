namespace SteamGuardDesktop.Domain.Confirmations
{
    /// <summary>
    /// Specifies the action applied to a pending Steam Guard confirmation.
    /// </summary>
    public enum ConfirmationDecisionType
    {
        /// <summary>
        /// Approves the pending confirmation.
        /// </summary>
        Approve = 0,

        /// <summary>
        /// Denies the pending confirmation.
        /// </summary>
        Deny = 1
    }
}
