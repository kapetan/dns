#if (!PORTABLE)
using System.Net.Sockets;
#endif
using System.Threading.Tasks;
using System.IO;
using DNS.Protocol;
using DNS.Protocol.Utils;
using System.Threading;

namespace DNS.Client.RequestResolver
{
    public class UdpRequestResolver : IRequestResolver
    {
        private const int TIMEOUT = 5000;

        private IRequestResolver fallback;

        public UdpRequestResolver(IRequestResolver fallback)
        {
            this.fallback = fallback;
        }

        public UdpRequestResolver()
        {
            this.fallback = new NullRequestResolver();
        }

        public async Task<ClientResponse> Request(ClientRequest request)
        {
            IPEndPoint dns = request.Dns;

#if (!PORTABLE)
            using(UdpClient udp = new UdpClient())
#else
            using (Sockets.Plugin.UdpSocketClient udp = new Sockets.Plugin.UdpSocketClient())

#endif
            {
#if (!PORTABLE)
                var bytes = request.ToArray();
                await udp
                    .SendAsync(bytes,request.Size, dns)
                    .WithCancellationTimeout(TIMEOUT);
#else
                string ip = request.Dns.Address.ToString();
                ip = request.Dns.Address.ToString().Replace(":", ".");
                await udp.ConnectAsync(ip, request.Dns.Port);
                await udp
                    .SendAsync(request.ToArray());
#endif
#if (!PORTABLE)
                UdpReceiveResult result = await udp.ReceiveAsync().WithCancellationTimeout(TIMEOUT);
                if(!result.RemoteEndPoint.ToString().Replace(".",":").Equals(dns.ToString())) throw new IOException("Remote endpoint mismatch");
                byte[] buffer = result.Buffer;
#else
                ManualResetEvent ev = new ManualResetEvent(false);

                byte[] buffer = null;
                udp.MessageReceived += (s, e) =>
                {
                    buffer = e.ByteData;
                    ev.Set();
                };
                if (!ev.WaitOne(TIMEOUT))
                    throw new System.TimeoutException();
#endif
                Response response = Response.FromArray(buffer);

                if (response.Truncated)
                {
                    return await fallback.Request(request);
                }

                return new ClientResponse(request, response, buffer);
            }
        }
    }
}
