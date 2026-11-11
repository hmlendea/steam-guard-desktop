namespace SteamGuardDesktop.Domain.Confirmations
{
    /// <summary>
    /// Categorises a pending Steam Guard confirmation using the identifiers reported by Steam.
    /// </summary>
    public enum ConfirmationType
    {
        /// <summary>
        /// The confirmation type could not be recognised, so no decision can be applied.
        /// </summary>
        Unsupported = 0,

        /// <summary>
        /// The confirmation is a test entry reported by Steam.
        /// </summary>
        Test = 1,

        /// <summary>
        /// The confirmation concerns a trade offer.
        /// </summary>
        Trade = 2,

        /// <summary>
        /// The confirmation concerns a community market sale.
        /// </summary>
        MarketSell = 3,

        /// <summary>
        /// The confirmation concerns opting out of a feature.
        /// </summary>
        FeatureOptOut = 4,

        /// <summary>
        /// The confirmation concerns a change of the registered phone number.
        /// </summary>
        PhoneNumberChange = 5,

        /// <summary>
        /// The confirmation concerns the recovery of an account.
        /// </summary>
        AccountRecovery = 6,

        /// <summary>
        /// The confirmation concerns the creation of a web API key.
        /// </summary>
        ApiKeyCreation = 9,

        /// <summary>
        /// The confirmation concerns joining a Steam family.
        /// </summary>
        JoinSteamFamily = 11
    }
}
