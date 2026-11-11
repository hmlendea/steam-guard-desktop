using System.Text.RegularExpressions;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Converts the HTML fragment reported by Steam into plain text that is safe to display.
    /// </summary>
    public static partial class ConfirmationDetailsTextConverter
    {
        /// <summary>
        /// Converts an HTML fragment into plain text without markup.
        /// </summary>
        /// <param name="html">The HTML fragment reported by Steam.</param>
        /// <returns>The plain text representation, or an empty string when no content is available.</returns>
        public static string ToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            string withoutScripts = ScriptBlockRegex().Replace(html, string.Empty);
            string withoutStyles = StyleBlockRegex().Replace(withoutScripts, string.Empty);
            string withoutTags = TagRegex().Replace(withoutStyles, " ");
            string decoded = System.Net.WebUtility.HtmlDecode(withoutTags);

            return WhitespaceRegex().Replace(decoded, " ").Trim();
        }

        [GeneratedRegex("<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
        private static partial Regex ScriptBlockRegex();

        [GeneratedRegex("<style[^>]*>.*?</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
        private static partial Regex StyleBlockRegex();

        [GeneratedRegex("<[^>]+>", RegexOptions.Singleline)]
        private static partial Regex TagRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();
    }
}
