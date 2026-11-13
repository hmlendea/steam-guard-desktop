using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using SteamGuardDesktop.Service.Diagnostics;
using SteamGuardDesktop.Service.Time;

namespace SteamGuardDesktop.IntegrationTests.Utils
{
    /// <summary>
    /// Provides a scripted HTTP message handler for deterministic protocol tests.
    /// </summary>
    internal sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> responses = [];

        private readonly Exception? failure;

        /// <summary>
        /// Initialises a new instance of the <see cref="RecordingHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="responses">The responses returned in order.</param>
        public RecordingHttpMessageHandler(IEnumerable<HttpResponseMessage> responses)
            => this.responses = new Queue<HttpResponseMessage>(responses);

        /// <summary>
        /// Initialises a new instance of the <see cref="RecordingHttpMessageHandler"/> class that fails.
        /// </summary>
        /// <param name="failure">The exception raised for every request.</param>
        public RecordingHttpMessageHandler(Exception failure) => this.failure = failure;

        /// <summary>
        /// Gets the requests that were issued.
        /// </summary>
        public List<HttpRequestMessage> Requests { get; } = [];

        /// <summary>
        /// Gets the request bodies that were issued.
        /// </summary>
        public List<string> RequestBodies { get; } = [];

        /// <summary>
        /// Creates a JSON response with the specified payload.
        /// </summary>
        /// <param name="payload">The JSON payload.</param>
        /// <returns>The response.</returns>
        public static HttpResponseMessage CreateJsonResponse(string payload) => new(HttpStatusCode.OK)
        {
            Content = new StringContent(payload)
        };

        /// <summary>
        /// Creates a response with the specified status code.
        /// </summary>
        /// <param name="statusCode">The status code of the response.</param>
        /// <returns>The response.</returns>
        public static HttpResponseMessage CreateStatusResponse(HttpStatusCode statusCode) => new(statusCode)
        {
            Content = new StringContent(string.Empty)
        };

        /// <inheritdoc />
        protected override HttpResponseMessage Send(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => SendAsync(request, cancellationToken).GetAwaiter().GetResult();

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            if (request.Content is not null)
            {
                RequestBodies.Add(request.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult());
            }

            if (failure is not null)
            {
                throw failure;
            }

            if (responses.Count == 0)
            {
                throw new InvalidOperationException("The handler received more requests than were scripted.");
            }

            return Task.FromResult(responses.Dequeue());
        }
    }
}
