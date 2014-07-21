using System;
using System.IO;
using System.Net.Sockets;
using Kapetan.Dns.Interface;
using Kapetan.Dns.Model;

namespace Kapetan.Dns.Resolver
{
    public class TcpRequestResolver : IRequestResolver
    {
        public ClientResponse Request(ClientRequest request)
        {
            var tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect(request.Dns);

                var stream = tcpClient.GetStream();
                var buffer = request.ToArray();
                var length = BitConverter.GetBytes((ushort)buffer.Length);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(length);
                }

                stream.Write(length, 0, length.Length);
                stream.Write(buffer, 0, buffer.Length);

                buffer = new byte[2];
                Read(stream, buffer);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(buffer);
                }

                buffer = new byte[BitConverter.ToUInt16(buffer, 0)];
                Read(stream, buffer);

                var response = Response.FromArray(buffer);

                return new ClientResponse(request, response, buffer);
            }
            finally
            {
                tcpClient.Close();
            }
        }

        private static void Read(Stream stream, byte[] buffer)
        {
            var length = buffer.Length;
            var offset = 0;
            var size = 0;

            while (length > 0 && (size = stream.Read(buffer, offset, length)) > 0)
            {
                offset += size;
                length -= size;
            }

            if (length > 0)
            {
                throw new IOException("Unexpected end of stream");
            }
        }
    }
}