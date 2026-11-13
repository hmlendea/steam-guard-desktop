using NuciLog.Core;

namespace SteamGuardDesktop.Configuration
{
    /// <summary>
    /// Represents the diagnostics settings of the application.
    /// </summary>
    public sealed class DiagnosticsSettings
    {
        /// <summary>
        /// Gets or sets the minimum severity of the messages that are written.
        /// </summary>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// Gets or sets the timestamp format used for each log line.
        /// </summary>
        public string TimestampFormat { get; set; } = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK";

        /// <summary>
        /// Gets or sets the format used for each log line.
        /// </summary>
        public string LogLineFormat { get; set; } = "{0}|{1}|{2}|{3}";

        /// <summary>
        /// Gets or sets the path of the file that receives the log lines.
        /// </summary>
        public string LogFilePath { get; set; } = "steamguarddesktop.log";

        /// <summary>
        /// Gets or sets a value indicating whether log lines are written to a file.
        /// </summary>
        public bool IsFileOutputEnabled { get; set; } = true;
    }
}
