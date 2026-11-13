namespace SteamGuardDesktop.Service.Storage
{
    /// <summary>
    /// Represents the output of a credential store utility invocation.
    /// </summary>
    public sealed class ProcessOutput
    {
        /// <summary>
        /// Gets or sets the process exit code.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Gets or sets the standard output of the process.
        /// </summary>
        public string StandardOutput { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the standard error of the process.
        /// </summary>
        public string StandardError { get; set; } = string.Empty;
    }
}
