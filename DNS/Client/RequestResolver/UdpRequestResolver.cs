using System.Net;
using System.Net.Sockets;
using DNS.Protocol;
using System.Threading.Tasks;

namespace DNS.Client.RequestResolver {
    public class UdpRequestResolver : IRequestResolver {
        private IRequestResolver fallback;

        public UdpRequestResolver(IRequestResolver fallback) {
            this.fallback = fallback;
        }

        public UdpRequestResolver() {
            this.fallback = new NullRequestResolver();
        }

        public async Task<ClientResponse> Request(ClientRequest request) {
            UdpClient udp = new UdpClient();
            IPEndPoint dns = request.Dns;

            try {
                udp.Client.SendTimeout = 5000;
                udp.Client.ReceiveTimeout = 5000;

                await udp.SendAsync(request.ToArray(), request.Size, dns);                

                byte[] buffer = (await udp.ReceiveAsync()).Buffer;
                Response response = Response.FromArray(buffer); //null;

                if (response.Truncated) {
                    return await fallback.Request(request);
                }

                return new ClientResponse(request, response, buffer);
            } finally {
                udp.Dispose();
            }
        }
    }
}
