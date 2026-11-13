using System.Collections.Generic;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Builds the JSON request payloads required by the Steam unified endpoints.
    /// </summary>
    public interface ISteamRequestPayloadBuilder
    {
        /// <summary>
        /// Builds the payload that requests the RSA public key of an account.
        /// </summary>
        /// <param name="accountName">The account name.</param>
        /// <returns>The JSON payload.</returns>
        string BuildPasswordRsaKeyPayload(string accountName);

        /// <summary>
        /// Builds the payload that starts a credential sign-in.
        /// </summary>
        /// <param name="accountName">The account name.</param>
        /// <param name="encryptedPassword">The RSA-encrypted, Base64-encoded password.</param>
        /// <param name="encryptionTimestamp">The timestamp that the RSA key was generated for.</param>
        /// <param name="deviceFriendlyName">The friendly name reported for this device.</param>
        /// <returns>The JSON payload.</returns>
        string BuildBeginCredentialSessionPayload(
            string accountName,
            string encryptedPassword,
            long encryptionTimestamp,
            string deviceFriendlyName);

        /// <summary>
        /// Builds the payload that submits a guard code for a pending sign-in.
        /// </summary>
        /// <param name="clientId">The identifier of the pending sign-in.</param>
        /// <param name="steamIdentifier">The Steam identifier of the account.</param>
        /// <param name="guardCode">The guard code.</param>
        /// <param name="guardType">The guard type that produced the code.</param>
        /// <returns>The JSON payload.</returns>
        string BuildGuardCodePayload(
            string clientId,
            string steamIdentifier,
            string guardCode,
            int guardType);

        /// <summary>
        /// Builds the payload that polls a pending sign-in.
        /// </summary>
        /// <param name="clientId">The identifier of the pending sign-in.</param>
        /// <param name="requestId">The opaque request value issued when the sign-in started.</param>
        /// <returns>The JSON payload.</returns>
        string BuildPollSessionPayload(string clientId, string requestId);
    }
}
