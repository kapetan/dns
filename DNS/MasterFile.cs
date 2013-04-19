using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using DNS.Protocol;

namespace DNS {
    public class MasterFile {
        private IList<IResourceRecord> entries = new List<IResourceRecord>();

        public void Add(IResourceRecord entry) {
            entries.Add(entry);
        }

        public void AddIPAddressResourceRecord(Domain domain, IPAddress ip) { }

        public void AddNameServerResourceRecord(Domain domain, Domain nsName) { }

        public IList<IResourceRecord> Get(Domain domain, RecordType type) {
            return entries.Where(e => e.Name.Equals(domain) && e.Type == type).ToList();
        }
    }
}
