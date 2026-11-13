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
        /// Gets the operation that applies the shared theme.
        /// </summary>
        public static Operation ApplyTheme => new SteamGuardOperation(nameof(ApplyTheme));

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
        /// Gets the operation that starts a sign-in.
        /// </summary>
        public static Operation BeginLogin => new SteamGuardOperation(nameof(BeginLogin));

        /// <summary>
        /// Gets the operation that submits a guard code for a sign-in.
        /// </summary>
        public static Operation SubmitLoginGuard => new SteamGuardOperation(nameof(SubmitLoginGuard));

        /// <summary>
        /// Gets the operation that polls a sign-in for its tokens.
        /// </summary>
        public static Operation PollLoginSession => new SteamGuardOperation(nameof(PollLoginSession));

        /// <summary>
        /// Gets the operation that enrols an authenticator.
        /// </summary>
        public static Operation AddAuthenticator => new SteamGuardOperation(nameof(AddAuthenticator));

        /// <summary>
        /// Gets the operation that removes an authenticator.
        /// </summary>
        public static Operation RemoveAuthenticator => new SteamGuardOperation(nameof(RemoveAuthenticator));

        /// <summary>
        /// Gets the operation that recovers an authenticator whose device is unavailable.
        /// </summary>
        public static Operation RecoverAuthenticator => new SteamGuardOperation(nameof(RecoverAuthenticator));

        /// <summary>
        /// Gets the operation that reports the authenticator status of an account.
        /// </summary>
        public static Operation ReadAuthenticatorStatus => new SteamGuardOperation(nameof(ReadAuthenticatorStatus));

        /// <summary>
        /// Gets the operation that signs every other Steam session out of an account.
        /// </summary>
        public static Operation RemoveAllGuardCookies => new SteamGuardOperation(nameof(RemoveAllGuardCookies));

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
