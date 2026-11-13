using System;

namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Provides access to the Secret Service credential store used by Linux desktop environments.
    /// </summary>
    public sealed class LinuxSecretServiceBackend : CommandLineSecretBackend
    {
        private static string AttributeName => "service";

        private static string AttributeValue => "SteamGuardDesktop";

        private static string LabelPrefix => "SteamGuardDesktop ";

        private static string ReachabilityProbeMarker => "SteamGuardDesktopReachabilityProbe";

        /// <inheritdoc />
        protected override string ExecutableName => "secret-tool";

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

            ProcessOutput? output = Run(ComposeStoreArguments(entryName), value);

            return output is not null && output.ExitCode == 0;
        }

        /// <inheritdoc />
        public override bool Delete(string entryName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entryName, nameof(entryName));

            ProcessOutput? output = Run(ComposeClearArguments(entryName), string.Empty);

            return output is not null && output.ExitCode == 0;
        }

        /// <inheritdoc />
        /// <remarks>
        /// A search for an attribute value that is never stored either reports no match while the Secret Service
        /// answers, or fails when the service is unreachable, so no stored secret is read by the probe.
        /// </remarks>
        protected override bool IsStoreReachable()
        {
            ProcessOutput? output = Run(ComposeReachabilityArguments(), string.Empty);

            return output is not null && output.ExitCode == 0;
        }

        private static string ComposeLookupArguments(string entryName) =>
            $"lookup {AttributeName} {AttributeValue} account {Quote(entryName)}";

        private static string ComposeReachabilityArguments() =>
            $"search {AttributeName} {ReachabilityProbeMarker}";

        private static string ComposeStoreArguments(string entryName) =>
            $"store --label {Quote(LabelPrefix + entryName)} {AttributeName} {AttributeValue} account {Quote(entryName)}";

        private static string ComposeClearArguments(string entryName) =>
            $"clear {AttributeName} {AttributeValue} account {Quote(entryName)}";

        private static string Quote(string value) => "\"" + value.Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
    }
}
