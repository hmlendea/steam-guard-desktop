using System;
using System.IO;

using NuciLog;
using NuciLog.Configuration;
using NuciLog.Core;

namespace SteamGuardDesktop.Logging
{
    /// <summary>
    /// Writes structured diagnostics to the console and to a log file.
    /// </summary>
    public sealed class SteamGuardLogger(NuciLoggerSettings settings) : Logger
    {
        /// <inheritdoc />
        protected override void WriteLog(LogLevel level, Func<string> logMessage)
        {
            if (level > settings.MinimumLevel)
            {
                return;
            }

            string timestamp = DateTime.Now.ToString(settings.TimestampFormat, System.Globalization.CultureInfo.InvariantCulture);
            string levelName = level.ToString().ToUpperInvariant();
            string formattedLine = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                settings.LogLineFormat,
                timestamp,
                SourceContext,
                levelName,
                logMessage());

            Console.WriteLine(formattedLine);

            if (!settings.IsFileOutputEnabled || string.IsNullOrWhiteSpace(settings.LogFilePath))
            {
                return;
            }

            File.AppendAllText(settings.LogFilePath, formattedLine + Environment.NewLine);
        }
    }
}
