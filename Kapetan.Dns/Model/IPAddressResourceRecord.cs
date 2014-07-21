using System;
using System.Net;
using Kapetan.Dns.Interface;

namespace Kapetan.Dns.Model
{
    public class IPAddressResourceRecord : ResourceRecordBase
    {
        private static IResourceRecord Create(Domain domain, IPAddress ip, TimeSpan ttl)
        {
            var data = ip.GetAddressBytes();
            var type = data.Length == 4 ? RecordType.A : RecordType.AAAA;

            return new ResourceRecord(domain, data, type, RecordClass.IN, ttl);
        }

        public IPAddressResourceRecord(IResourceRecord record)
            : base(record)
        {
            IPAddress = new IPAddress(Data);
        }

        public IPAddressResourceRecord(Domain domain, IPAddress ip, TimeSpan ttl = default(TimeSpan)) :
            base(Create(domain, ip, ttl))
        {
            IPAddress = ip;
        }

        public IPAddress IPAddress
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Dump().Add("IPAddress").ToString();
        }
    }
}
