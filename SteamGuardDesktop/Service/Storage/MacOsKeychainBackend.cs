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

        private static string ComposeLookupArguments(string entryName) =>
            $"find-generic-password -s {Quote(ServiceName)} -a {Quote(AccountPrefix + entryName)} -w";

        private static string ComposeStoreArguments(string entryName) =>
            $"add-generic-password -U -s {Quote(ServiceName)} -a {Quote(AccountPrefix + entryName)} -w -T \"\"";

        private static string ComposeDeleteArguments(string entryName) =>
            $"delete-generic-password -s {Quote(ServiceName)} -a {Quote(AccountPrefix + entryName)}";

        private static string Quote(string value) => "\"" + value.Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
    }
}
