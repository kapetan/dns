﻿using System;
using System.Collections.Generic;
using System.Linq;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Client.RequestResolver;
using System.Threading.Tasks;

namespace DNS.Client
{
    public class DnsClient
    {
        private const int DEFAULT_PORT = 53;
        private static readonly Random RANDOM = new Random();
        
        private IPEndPoint dns;
        private IRequestResolver resolver;
        public DnsClient(IPEndPoint dns, IRequestResolver resolver = null)
        {

            this.dns = dns;
            this.resolver = resolver == null ? new UdpRequestResolver(new TcpRequestResolver()) : resolver;
        }

        public DnsClient(IPAddress ip, int port = DEFAULT_PORT, IRequestResolver resolver = null) :
            this(new IPEndPoint(ip, port), resolver) { }

        public DnsClient(string ip, int port = DEFAULT_PORT, IRequestResolver resolver = null) :
            this(IPAddress.Parse(ip), port, resolver) { }
        public ClientRequest FromArray(byte[] message)
        {
            Request request = Request.FromArray(message);
            return new ClientRequest(dns, request, resolver);
        }

        public ClientRequest Create(IRequest request = null)
        {
            return new ClientRequest(dns, request, resolver);
        }
        
        public async Task<IList<IPAddress>> Lookup(string domain, RecordType type = RecordType.A)
        {
            if (type != RecordType.A && type != RecordType.AAAA)
            {
                throw new ArgumentException("Invalid record type " + type);
            }

            ClientResponse response = await Resolve(domain, type);
            IList<IPAddress> ips = response.AnswerRecords
                .Where(r => r.Type == type)
                .Cast<IPAddressResourceRecord>()
                .Select(r => r.IPAddress)
                .ToList();

            if (ips.Count == 0)
            {
                throw new ResponseException(response, "No matching records");
            }

            return ips;
        }
#if (!PORTABLE)
        public Task<string> Reverse(string ip)
        {
            return Reverse(IPAddress.Parse(ip));
        }
#endif

#if (!PORTABLE)
        public async Task<string> Reverse(IPAddress ip)
        {
            ClientResponse response = await Resolve(Domain.PointerName(ip), RecordType.PTR);
            IResourceRecord ptr = response.AnswerRecords.FirstOrDefault(r => r.Type == RecordType.PTR);

            if (ptr == null)
            {
                throw new ResponseException(response, "No matching records");
            }

            return ((PointerResourceRecord)ptr).PointerDomainName.ToString();
        }
#endif
        public Task<ClientResponse> Resolve(string domain, RecordType type)
        {
            return Resolve(new Domain(domain), type);
        }

        public Task<ClientResponse> Resolve(Domain domain, RecordType type)
        {
            ClientRequest request = Create();
            Question question = new Question(domain, type);

            request.Questions.Add(question);
            request.OperationCode = OperationCode.Query;
            request.RecursionDesired = true;

            return request.Resolve();
        }
    }
}
