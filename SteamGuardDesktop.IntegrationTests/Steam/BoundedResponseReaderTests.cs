using System;
using System.IO;
using System.Net;
using System.Net.Http;

using NUnit.Framework;

using SteamGuardDesktop.Client.Steam;

namespace SteamGuardDesktop.IntegrationTests.Steam
{
    [TestFixture]
    public sealed class BoundedResponseReaderTests
    {
        [Test]
        public void GivenASmallResponse_WhenRead_ThenTheWholeBodyIsReturned()
        {
            using HttpResponseMessage response = CreateResponse("{\"success\":true}");

            string? payload = BoundedResponseReader.ReadAsString(response, 1024);

            Assert.That(payload, Is.EqualTo("{\"success\":true}"));
        }

        [Test]
        public void GivenAResponseExactlyAtTheLimit_WhenRead_ThenItIsAccepted()
        {
            string body = new('a', 64);

            using HttpResponseMessage response = CreateResponse(body);

            string? payload = BoundedResponseReader.ReadAsString(response, 64);

            Assert.That(payload, Is.EqualTo(body));
        }

        [Test]
        public void GivenAResponseOneByteOverTheLimit_WhenRead_ThenItIsRejected()
        {
            string body = new('a', 65);

            using HttpResponseMessage response = CreateResponse(body);

            string? payload = BoundedResponseReader.ReadAsString(response, 64);

            Assert.That(payload, Is.Null);
        }

        [Test]
        public void GivenADeclaredLengthOverTheLimit_WhenRead_ThenItIsRejectedWithoutReadingTheBody()
        {
            using HttpResponseMessage response = CreateResponse(new string('a', 65));

            response.Content.Headers.ContentLength = 65;

            string? payload = BoundedResponseReader.ReadAsString(response, 64);

            Assert.That(payload, Is.Null);
        }

        [Test]
        public void GivenALargeStreamedBodyWithoutDeclaredLength_WhenRead_ThenItStopsAtTheLimit()
        {
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StreamContent(new EndlessStream())
            };

            response.Content.Headers.ContentLength = null;

            string? payload = BoundedResponseReader.ReadAsString(response, 4096);

            Assert.That(payload, Is.Null);
            Assert.That(EndlessStream.BytesRead, Is.LessThan(200000));
        }

        [Test]
        public void GivenAnEmptyResponse_WhenRead_ThenAnEmptyStringIsReturned()
        {
            using HttpResponseMessage response = CreateResponse(string.Empty);

            string? payload = BoundedResponseReader.ReadAsString(response, 64);

            Assert.That(payload, Is.Empty);
        }

        [Test]
        public void GivenAWhitespaceLimit_WhenRead_ThenTheResponseIsRejected()
        {
            using HttpResponseMessage response = CreateResponse("content");

            Assert.That(BoundedResponseReader.ReadAsString(response, 0), Is.Null);
            Assert.That(BoundedResponseReader.ReadAsString(response, -1), Is.Null);
        }

        [Test]
        public void GivenAMissingResponse_WhenRead_ThenItIsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => BoundedResponseReader.ReadAsString(null!, 64));
        }

        private static HttpResponseMessage CreateResponse(string body) => new(HttpStatusCode.OK)
        {
            Content = new StringContent(body)
        };

        private sealed class EndlessStream : Stream
        {
            public static long BytesRead { get; private set; }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => BytesRead;
                set => throw new NotSupportedException();
            }

            public override void Flush() => BytesRead = 0;

            public override int Read(byte[] buffer, int offset, int count)
            {
                BytesRead += count;

                return count;
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
