using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Kapetan.Dns.Interface;
using Kapetan.Dns.Model;
using Kapetan.Dns.Resolver;

namespace Kapetan.Dns
{
    public class Client
    {
        private const int DEFAULT_PORT = 53;
        private static readonly Random RANDOM = new Random();

        private IPEndPoint dns;
        private IRequestResolver resolver;

        public Client(IPEndPoint dns, IRequestResolver resolver = null)
        {
            this.dns = dns;
            this.resolver = resolver == null ? new UdpRequestResolver(new TcpRequestResolver()) : resolver;
        }

        public Client(IPAddress ip, int port = DEFAULT_PORT, IRequestResolver resolver = null) :
            this(new IPEndPoint(ip, port), resolver) { }

        public Client(string ip, int port = DEFAULT_PORT, IRequestResolver resolver = null) :
            this(IPAddress.Parse(ip), port, resolver) { }

        public ClientRequest FromArray(byte[] message)
        {
            var request = Request.FromArray(message);
            return new ClientRequest(this.dns, request, this.resolver);
        }

        public ClientRequest Create(IRequest request = null)
        {
            return new ClientRequest(this.dns, request, this.resolver);
        }

        public IList<IPAddress> Lookup(string domain, RecordType type = RecordType.A)
        {
            if (type != RecordType.A && type != RecordType.AAAA)
            {
                throw new ArgumentException("Invalid record type " + type);
            }

            var response = Resolve(domain, type);
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

        public string Reverse(string ip)
        {
            return Reverse(IPAddress.Parse(ip));
        }

        public string Reverse(IPAddress ip)
        {
            var response = this.Resolve(Domain.PointerName(ip), RecordType.PTR);
            var ptr = response.AnswerRecords.FirstOrDefault(r => r.Type == RecordType.PTR);

            if (ptr == null)
            {
                throw new ResponseException(response, "No matching records");
            }

            return ((PointerResourceRecord)ptr).PointerDomainName.ToString();
        }

        public ClientResponse Resolve(string domain, RecordType type)
        {
            return this.Resolve(new Domain(domain), type);
        }

        public ClientResponse Resolve(Domain domain, RecordType type)
        {
            var request = this.Create();
            var question = new Question(domain, type);

            request.Questions.Add(question);
            request.OperationCode = OperationCode.Query;
            request.RecursionDesired = true;

            return request.Resolve();
        }
    }
}
