using System.Collections.Generic;

using SteamGuardDesktop.Service.Diagnostics;

namespace SteamGuardDesktop.IntegrationTests.Utils
{
    /// <summary>
    /// Records the addresses that were offered for redaction.
    /// </summary>
    internal sealed class RecordingSecretRedactor : ISecretRedactor
    {
        private readonly ISecretRedactor innerRedactor = new SecretRedactor();

        /// <summary>
        /// Gets the addresses that were offered for redaction.
        /// </summary>
        public List<string> SanitisedAddresses { get; } = [];

        /// <inheritdoc />
        public string SanitiseAddress(string address)
        {
            SanitisedAddresses.Add(address);

            return innerRedactor.SanitiseAddress(address);
        }
    }
}
