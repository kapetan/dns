using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using DNS.Protocol;
using DNS.Protocol.Utils;

namespace DNS.Client.RequestResolver {
    public class UdpRequestResolver : IRequestResolver {
        private const int TIMEOUT = 5000;

        private IRequestResolver fallback;

        public UdpRequestResolver(IRequestResolver fallback) {
            this.fallback = fallback;
        }

        public UdpRequestResolver() {
            this.fallback = new NullRequestResolver();
        }

        public async Task<ClientResponse> Request(ClientRequest request) {
            IPEndPoint dns = request.Dns;

            using(UdpClient udp = new UdpClient()) {
                await udp
                    .SendAsync(request.ToArray(), request.Size, dns)
                    .WithCancellationTimeout(TIMEOUT);

                UdpReceiveResult result = await udp.ReceiveAsync().WithCancellationTimeout(TIMEOUT);
                if(!result.RemoteEndPoint.Equals(dns)) throw new IOException("Remote endpoint mismatch");
                byte[] buffer = result.Buffer;
                Response response = Response.FromArray(buffer);

                if (response.Truncated) {
                    return await fallback.Request(request);
                }

                return new ClientResponse(request, response, buffer);
            }
        }
    }
}
