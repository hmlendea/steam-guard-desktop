using System;

namespace SteamGuardDesktop.Service.Diagnostics
{
    /// <summary>
    /// Removes sensitive values from request addresses.
    /// </summary>
    public sealed class SecretRedactor : ISecretRedactor
    {
        private static string MaskText => "********";

        private static string RedactedAddressSuffix => "?[redacted]";

        /// <inheritdoc />
        public string SanitiseAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return string.Empty;
            }

            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri? parsedAddress))
            {
                return MaskText;
            }

            string authority = parsedAddress.GetLeftPart(UriPartial.Path);

            return authority + RedactedAddressSuffix;
        }
    }
}
