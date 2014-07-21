using System.Net;
using System.Net.Sockets;
using Kapetan.Dns.Model;
using Kapetan.Dns.Interface;

namespace Kapetan.Dns.Resolver
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
            var udp = new UdpClient();
            var dns = request.Dns;

            try
            {
                udp.Connect(dns);
                udp.Send(request.ToArray(), request.Size);

                byte[] buffer = udp.Receive(ref dns);
                var response = Response.FromArray(buffer); //null;

                if (response.Truncated)
                {
                    return fallback.Request(request);
                }

                return new ClientResponse(request, response, buffer);
            }
            finally
            {
                udp.Close();
            }
        }
    }
}
