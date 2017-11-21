using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using DNS.Protocol;
using DNS.Protocol.Utils;

namespace DNS.Client.RequestResolver {
    public class UdpRequestResolver : IRequestResolver {
        private readonly int timeout;

        private IRequestResolver fallback;

        public UdpRequestResolver(IRequestResolver fallback, int timeout = 5000) {
            this.fallback = fallback;
            this.timeout = timeout;
        }

        public UdpRequestResolver(int timeout = 5000) {
            this.fallback = new NullRequestResolver();
            this.timeout = timeout;
        }

        public async Task<ClientResponse> Request(ClientRequest request) {
            IPEndPoint dns = request.Dns;

            using(UdpClient udp = new UdpClient()) {
                await udp
                    .SendAsync(request.ToArray(), request.Size, dns)
                    .WithCancellationTimeout(timeout);

                UdpReceiveResult result = await udp.ReceiveAsync().WithCancellationTimeout(timeout);
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
