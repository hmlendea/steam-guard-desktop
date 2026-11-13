using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace SteamGuardDesktop.Client.Steam
{
    /// <summary>
    /// Reads HTTP response bodies while refusing to buffer more than an accepted number of bytes.
    /// </summary>
    /// <remarks>
    /// Reading the body through <see cref="HttpContent.ReadAsStringAsync()"/> buffers the entire response before
    /// any size check can run, so a hostile or broken peer could still force an unbounded allocation. This reader
    /// stops as soon as the accepted size is exceeded.
    /// </remarks>
    public static class BoundedResponseReader
    {
        private static int BufferSizeBytes => 8192;

        /// <summary>
        /// Reads the response body as text, up to the accepted number of bytes.
        /// </summary>
        /// <param name="response">The HTTP response to read.</param>
        /// <param name="maximumBytes">The maximum number of bytes accepted.</param>
        /// <returns>The response body as text, or <c>null</c> when it exceeds the accepted size.</returns>
        public static string? ReadAsString(HttpResponseMessage response, int maximumBytes)
        {
            ArgumentNullException.ThrowIfNull(response, nameof(response));

            if (maximumBytes <= 0)
            {
                return null;
            }

            long? declaredLength = response.Content.Headers.ContentLength;

            if (declaredLength.HasValue && declaredLength.Value > maximumBytes)
            {
                return null;
            }

            using Stream responseStream = response.Content.ReadAsStream();
            using MemoryStream buffer = new();
            byte[] chunk = new byte[BufferSizeBytes];

            int readBytes;

            while ((readBytes = responseStream.Read(chunk, 0, chunk.Length)) > 0)
            {
                if (buffer.Length + readBytes > maximumBytes)
                {
                    return null;
                }

                buffer.Write(chunk, 0, readBytes);
            }

            return Encoding.UTF8.GetString(buffer.ToArray());
        }
    }
}
