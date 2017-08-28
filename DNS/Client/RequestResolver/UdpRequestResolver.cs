using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using DNS.Protocol;
using DNS.Protocol.Utils;

namespace DNS.Client.RequestResolver {
    public class UdpRequestResolver : IRequestResolver {
        private const int TIMEOUT = 5000;

        private readonly IRequestResolver fallback;

        public UdpRequestResolver(IRequestResolver fallback) {
            this.fallback = fallback;
        }

        public UdpRequestResolver() {
            this.fallback = new NullRequestResolver();
        }

        public async Task<ClientResponse> Request(ClientRequest request, CancellationToken cancellationToken = default(CancellationToken)) {
            IPEndPoint dns = request.Dns;

            using(UdpClient udp = new UdpClient()) {
                await udp
                    .SendAsync(request.ToArray(), request.Size, dns)
                    .WithCancellationOrTimeout(cancellationToken, TimeSpan.FromMilliseconds(TIMEOUT));
                
                UdpReceiveResult result = await udp.ReceiveAsync().WithCancellationOrTimeout(cancellationToken, TimeSpan.FromMilliseconds(TIMEOUT));
                if(!result.RemoteEndPoint.Equals(dns)) throw new IOException("Remote endpoint mismatch");
                byte[] buffer = result.Buffer;
                Response response = Response.FromArray(buffer);

                if (response.Truncated) {
                    return await fallback.Request(request, cancellationToken);
                }

                return new ClientResponse(request, response, buffer);
            }
        }
    }
}
