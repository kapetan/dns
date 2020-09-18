using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DNS.Protocol;

namespace DNS.Client.RequestResolver
{
    public enum DoHProtocolMode
    {
        Rfc8484Post,
        Rfc8484Get,
        CloudflareJson,
    }

    public class DoHRequestResolver : IRequestResolver
    {
        private const string mime = "application/dns-message";
        private static readonly HttpClient http = new HttpClient();
        private readonly Uri dns;
        private readonly DoHProtocolMode mode;

        public DoHRequestResolver(string dns, DoHProtocolMode mode = DoHProtocolMode.Rfc8484Post)
            : this(new Uri(dns), mode)
        {
        }

        public DoHRequestResolver(Uri dns, DoHProtocolMode mode = DoHProtocolMode.Rfc8484Post)
        {
            this.dns = dns;
            this.mode = mode;
        }

        public async Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default)
        {
            // set id to 0, rfc8484 4.1
            int id = request.Id;
            request.Id = 0;

            IResponse response;
            switch (mode)
            {
                case DoHProtocolMode.Rfc8484Get:
                    {
                        UriBuilder ub = new UriBuilder(dns);
                        System.Collections.Specialized.NameValueCollection query = HttpUtility.ParseQueryString(dns.Query);

                        byte[] reqByte = request.ToArray();
                        string reqBase64 = Convert.ToBase64String(reqByte)
                                .TrimEnd('=')
                                .Replace('+', '-')
                                .Replace('/', '_');
                        query["dns"] = reqBase64;
                        ub.Query = query.ToString();
                        // GET /dns-query?dns={base64url(query)} HTTP/2
                        // Accept: application/dns-message
                        //
                        HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Get, ub.Uri);
                        reqMsg.Headers.Clear();
                        reqMsg.Headers.Add("Accept", mime);

                        HttpResponseMessage hrm = await http.SendAsync(reqMsg, cancellationToken);
                        if (!hrm.IsSuccessStatusCode)
                        {
                            throw new WebException($"DoH server return code {hrm.StatusCode}");
                        }

                        byte[] respByte = await hrm.Content.ReadAsByteArrayAsync();
                        response = Response.FromArray(respByte);

                        break;
                    }
                case DoHProtocolMode.Rfc8484Post:
                    {
                        byte[] reqByte = request.ToArray();

                        // POST /dns-query HTTP/2
                        // Accept: application/dns-message
                        // Content-Type: application/dns-message
                        // Content-Length: {query.length}
                        //
                        // {query}

                        HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Post, dns);
                        reqMsg.Headers.Clear();
                        reqMsg.Headers.Add("Accept", mime);
                        reqMsg.Headers.Add("Content-Type", mime);
                        reqMsg.Headers.Add("Content-Length", reqByte.Length.ToString());
                        reqMsg.Content = new ByteArrayContent(reqByte);

                        HttpResponseMessage hrm = await http.SendAsync(reqMsg, cancellationToken);
                        if (!hrm.IsSuccessStatusCode)
                        {
                            throw new WebException($"DoH server return code {hrm.StatusCode}");
                        }

                        byte[] respByte = await hrm.Content.ReadAsByteArrayAsync();
                        response = Response.FromArray(respByte);
                        break;
                    }
                case DoHProtocolMode.CloudflareJson:
                    {
                        throw new NotImplementedException();
                    }
                default:
                    throw new NotSupportedException();
            }

            // recover original id
            request.Id = id;
            response.Id = id;
            return new ClientResponse(request, response);
        }
    }
}
