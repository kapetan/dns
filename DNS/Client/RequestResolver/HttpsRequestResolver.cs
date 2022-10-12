using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using DNS.Protocol;
using DNS.Protocol.Utils;

namespace DNS.Client.RequestResolver {
    public class HttpsRequestResolver : IRequestResolver {
        private int timeout;
        private IRequestResolver fallback;
        private HttpClient httpClient;
        private void SetUpClient(Uri uri)
        {
            httpClient = new HttpClient()
            {
                BaseAddress = uri,
                Timeout = TimeSpan.FromMilliseconds(timeout)
                //DefaultRequestVersion = new Version(2, 0),
            };
            httpClient.DefaultRequestHeaders.Add("Accept", "application/dns-message");
        }
        public HttpsRequestResolver(Uri uri, IRequestResolver fallback, int timeout = 5000) {
            this.fallback = fallback;
            this.timeout = timeout;
            SetUpClient(uri);
        }

        public HttpsRequestResolver(Uri uri, int timeout = 5000) {
            this.fallback = new NullRequestResolver();
            this.timeout = timeout;
            SetUpClient(uri);
        }

        public async Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default(CancellationToken)) {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "")
            {
                Content = new ByteArrayContent(request.ToArray(), 0, request.Size)
            };

            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/dns-message");
            var httpResponse = await httpClient.SendAsync(httpRequest).WithCancellationTimeout(TimeSpan.FromMilliseconds(timeout), cancellationToken).ConfigureAwait(false);
            Byte[] buffer = await httpResponse.Content.ReadAsByteArrayAsync();
            Response response = Response.FromArray(buffer);

            if (response.Truncated)
            {
                return await fallback.Resolve(request, cancellationToken).ConfigureAwait(false);
            }

            return new ClientResponse(request, response, buffer);
        }
    }
}
