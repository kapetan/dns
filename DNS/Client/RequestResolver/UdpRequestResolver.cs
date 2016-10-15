using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DNS.Protocol;

namespace DNS.Client.RequestResolver
{
    public class UdpRequestResolver : IRequestResolver
    {
        private IRequestResolver fallback;

        public UdpRequestResolver(IRequestResolver fallback)
        {
            this.fallback = fallback;
        }

        public UdpRequestResolver()
        {
            this.fallback = new NullRequestResolver();
        }

        public ClientResponse Request(ClientRequest request)
        {
            IPEndPoint dns = request.Dns;

            using (UdpClient udp = new UdpClient())
            {
                udp.Client.SendTimeout = 5000;
                udp.Client.ReceiveTimeout = 5000;

                udp.Connect(dns);
                udp.Send(request.ToArray(), request.Size);

                byte[] buffer = udp.Receive(ref dns);
                Response response = Response.FromArray(buffer); //null;

                if (response.Truncated)
                {
                    return fallback.Request(request);
                }

                return new ClientResponse(request, response, buffer);
            }
        }

#if NET45
        public async Task<ClientResponse> RequestAsync(ClientRequest request)
        {
            using (var udp = new UdpClient()) //it's quite bad for perfomance, it should be field
            {
                IPEndPoint dns = request.Dns;
                udp.Client.SendTimeout = 5000;
                udp.Client.ReceiveTimeout = 5000;

                udp.Connect(dns);
                await udp.SendAsync(request.ToArray(), request.Size);

                var result = await udp.ReceiveAsync();
                Response response = Response.FromArray(result.Buffer); //null;

                if (response.Truncated)
                {
                    return fallback.Request(request);
                }

                return new ClientResponse(request, response, result.Buffer);
            }
        }
#endif
    }
}
