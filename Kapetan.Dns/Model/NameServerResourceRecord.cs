using System;
using Kapetan.Dns.Interface;

namespace Kapetan.Dns.Model
{
    public class NameServerResourceRecord : ResourceRecordBase
    {
        public NameServerResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            NSDomainName = Domain.FromArray(message, dataOffset);
        }

        public NameServerResourceRecord(Domain domain, Domain nsDomain, TimeSpan ttl = default(TimeSpan)) :
            base(new ResourceRecord(domain, nsDomain.ToArray(), RecordType.NS, RecordClass.IN, ttl))
        {
            NSDomainName = nsDomain;
        }

        public Domain NSDomainName
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Dump().Add("NSDomainName").ToString();
        }
    }
}
