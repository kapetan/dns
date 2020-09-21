﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using DNS.Protocol;
using System.Net.Security;

namespace DNS.Client.RequestResolver
{
    public class TcpRequestResolver : IRequestResolver
    {
        private IPEndPoint dns;
        private bool tls;

        public TcpRequestResolver(IPEndPoint dns, bool tls = false)
        {
            this.dns = dns;
            this.tls = tls;
            if (!tls && dns.Port == 853)
            {
                throw new NotSupportedException("DNS clients and servers MUST NOT use port 853 to transport cleartext DNS messages");
            }
        }

        public async Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (TcpClient tcp = new TcpClient(dns.AddressFamily))
            {
                await tcp.ConnectAsync(dns.Address, dns.Port);

                NetworkStream tcpStream = tcp.GetStream();
                SslStream tlsStream = null;
                if (tls)
                {
                    tlsStream = new SslStream(tcpStream);
                    await tlsStream.AuthenticateAsClientAsync(dns.Address.ToString());
                }
                using (Stream stream = tls ? tlsStream : (Stream)tcpStream)
                {
                    byte[] buffer = request.ToArray();
                    byte[] length = BitConverter.GetBytes((ushort)buffer.Length);

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(length);
                    }

                    await stream.WriteAsync(length, 0, length.Length, cancellationToken);
                    await stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);

                    buffer = new byte[2];
                    await Read(stream, buffer, cancellationToken);

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(buffer);
                    }

                    buffer = new byte[BitConverter.ToUInt16(buffer, 0)];
                    await Read(stream, buffer, cancellationToken);

                    IResponse response = Response.FromArray(buffer);
                    return new ClientResponse(request, response, buffer);
                }
            }
        }

        private static async Task Read(Stream stream, byte[] buffer, CancellationToken cancellationToken)
        {
            int length = buffer.Length;
            int offset = 0;
            int size = 0;

            while (length > 0 && (size = await stream.ReadAsync(buffer, offset, length, cancellationToken)) > 0)
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
