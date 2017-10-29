using System;
using System.IO;
#if (!PORTABLE)
using System.Net.Sockets;
#endif
using DNS.Protocol;
using System.Threading.Tasks;

namespace DNS.Client.RequestResolver {
    public class TcpRequestResolver : IRequestResolver {
        public async Task<ClientResponse> Request(ClientRequest request) {
            IPEndPoint dns = request.Dns;

#if (!PORTABLE)
            using (TcpClient tcp = new TcpClient()) {
#else
            using (Sockets.Plugin.TcpSocketClient tcp = new Sockets.Plugin.TcpSocketClient())
            {

#endif
                await tcp.ConnectAsync(dns.Address.ToString(), dns.Port);

#if (!PORTABLE)
                Stream readStream = tcp.GetStream();
                Stream writeStream = tcp.GetStream();
#else
                Stream readStream = tcp.ReadStream;
                Stream writeStream = tcp.WriteStream;
#endif
                byte[] buffer = request.ToArray();
                byte[] length = BitConverter.GetBytes((ushort) buffer.Length);

                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(length);
                }

                await writeStream.WriteAsync(length, 0, length.Length);
                await writeStream.WriteAsync(buffer, 0, buffer.Length);

                buffer = new byte[2];
                await Read(readStream, buffer);

                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(buffer);
                }

                buffer = new byte[BitConverter.ToUInt16(buffer, 0)];
                await Read(readStream, buffer);

                Response response = Response.FromArray(buffer);

                return new ClientResponse(request, response, buffer);
            }
        }

        private static async Task Read(Stream stream, byte[] buffer) {
            int length = buffer.Length;
            int offset = 0;
            int size = 0;

            while (length > 0 && (size = await stream.ReadAsync(buffer, offset, length)) > 0) {
                offset += size;
                length -= size;
            }

            if (length > 0) {
                throw new IOException("Unexpected end of stream");
            }
        }
    }
}
