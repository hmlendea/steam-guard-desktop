using NuciLog.Core;

namespace SteamGuardDesktop.Logging
{
    /// <summary>
    /// Defines the operations reported to the diagnostics log.
    /// </summary>
    public sealed class SteamGuardOperation : Operation
    {
        /// <summary>
        /// Gets the application launch operation.
        /// </summary>
        public static Operation LaunchApplication => new SteamGuardOperation(nameof(LaunchApplication));

        /// <summary>
        /// Gets the operation that resolves and applies the platform theme.
        /// </summary>
        public static Operation ResolveTheme => new SteamGuardOperation(nameof(ResolveTheme));

        /// <summary>
        /// Gets the operation that reads the credential store status.
        /// </summary>
        public static Operation ReadSecretStoreStatus => new SteamGuardOperation(nameof(ReadSecretStoreStatus));

        /// <summary>
        /// Gets the operation that stores account secrets.
        /// </summary>
        public static Operation StoreSecrets => new SteamGuardOperation(nameof(StoreSecrets));

        /// <summary>
        /// Gets the operation that deletes account secrets.
        /// </summary>
        public static Operation DeleteSecrets => new SteamGuardOperation(nameof(DeleteSecrets));

        /// <summary>
        /// Gets the operation that generates an authenticator code.
        /// </summary>
        public static Operation GenerateCode => new SteamGuardOperation(nameof(GenerateCode));

        /// <summary>
        /// Gets the operation that loads pending confirmations.
        /// </summary>
        public static Operation LoadConfirmations => new SteamGuardOperation(nameof(LoadConfirmations));

        /// <summary>
        /// Gets the operation that loads the details of a pending confirmation.
        /// </summary>
        public static Operation LoadConfirmationDetails => new SteamGuardOperation(nameof(LoadConfirmationDetails));

        /// <summary>
        /// Gets the operation that applies a confirmation decision.
        /// </summary>
        public static Operation ApplyConfirmationDecision => new SteamGuardOperation(nameof(ApplyConfirmationDecision));

        /// <summary>
        /// Gets the operation that signs a confirmation request.
        /// </summary>
        public static Operation SignConfirmationRequest => new SteamGuardOperation(nameof(SignConfirmationRequest));

        /// <summary>
        /// Gets the operation that sends a request to Steam.
        /// </summary>
        public static Operation SendSteamRequest => new SteamGuardOperation(nameof(SendSteamRequest));

        private SteamGuardOperation(string name)
            : base(name)
        {
        }
    }
}
