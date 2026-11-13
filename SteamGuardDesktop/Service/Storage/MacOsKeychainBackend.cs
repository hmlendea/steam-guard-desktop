using System;

namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Provides access to the macOS Keychain.
    /// </summary>
    public sealed class MacOsKeychainBackend : CommandLineSecretBackend
    {
        private static string ServiceName => "SteamGuardDesktop";

        private static string AccountPrefix => "SteamGuardDesktop:";

        private static string ReachabilityProbeMarker => "SteamGuardDesktopReachabilityProbe";

        private static int ErrorItemNotFound => 44;

        /// <inheritdoc />
        protected override string ExecutableName => "security";

        /// <inheritdoc />
        public override string? Read(string entryName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryName, nameof(entryName));

            ProcessOutput? output = Run(ComposeLookupArguments(entryName), string.Empty);

            if (output is null || output.ExitCode != 0)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(output.StandardOutput))
            {
                return null;
            }

            return output.StandardOutput.TrimEnd('\n');
        }

        /// <inheritdoc />
        public override bool Write(string entryName, string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryName, nameof(entryName));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            Delete(entryName);

            ProcessOutput? output = Run(ComposeStoreArguments(entryName), value);

            return output is not null && output.ExitCode == 0;
        }

        /// <inheritdoc />
        public override bool Delete(string entryName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryName, nameof(entryName));

            ProcessOutput? output = Run(ComposeDeleteArguments(entryName), string.Empty);

            return output is not null;
        }

        /// <inheritdoc />
        /// <remarks>
        /// A lookup of an entry that is never stored reports that the item was not found while the Keychain
        /// answers, and fails with any other status when the Keychain cannot be used, so no stored secret is read
        /// by the probe.
        /// </remarks>
        protected override bool IsStoreReachable()
        {
            ProcessOutput? output = Run(ComposeLookupArguments(ReachabilityProbeMarker), string.Empty);

            if (output is null)
            {
                return false;
            }

            return output.ExitCode == 0 || output.ExitCode == ErrorItemNotFound;
        }

        private static string ComposeLookupArguments(string entryName) =>
            $"find-generic-password -s {Quote(ServiceName)} -a {Quote(AccountPrefix + entryName)} -w";

        private static string ComposeStoreArguments(string entryName) =>
            $"add-generic-password -U -s {Quote(ServiceName)} -a {Quote(AccountPrefix + entryName)} -w -T \"\"";

        private static string ComposeDeleteArguments(string entryName) =>
            $"delete-generic-password -s {Quote(ServiceName)} -a {Quote(AccountPrefix + entryName)}";

        private static string Quote(string value) => "\"" + value.Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
    }
}
