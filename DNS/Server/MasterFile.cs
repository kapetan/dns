using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace DNS.Server {
    public class MasterFile : MasterFileResolver {
        private static readonly TimeSpan DEFAULT_TTL = new TimeSpan(0);

        private class Entry : IEntry {
            private readonly IResourceRecord record;

            public Entry(IResourceRecord record) {
                this.record = record;
            }

            public IResourceRecord Record => this.record;

            public bool IsMatch(Question question) {
                var labels = question.Name.ToString().Split('.');
                var patterns = labels.Select(l => l == "*" ? @"(\w+)" : Regex.Escape(l)).ToArray();
                var re = new Regex("^" + String.Join(@"\.", patterns) + "$");

                return question.Type == record.Type && re.IsMatch(question.Name.ToString());
            }
        }

        private TimeSpan ttl = DEFAULT_TTL;

        public MasterFile(TimeSpan ttl) {
            this.ttl = ttl;
        }

        public MasterFile() { }

        public void Add(IResourceRecord record) {
            Entries.Add(new Entry(record));
        }

        public void AddIPAddressResourceRecord(string domain, string ip) {
            AddIPAddressResourceRecord(new Domain(domain), IPAddress.Parse(ip));
        }

        public void AddIPAddressResourceRecord(Domain domain, IPAddress ip) {
            Add(new IPAddressResourceRecord(domain, ip, ttl));
        }

        public void AddNameServerResourceRecord(string domain, string nsDomain) {
            AddNameServerResourceRecord(new Domain(domain), new Domain(nsDomain));
        }

        public void AddNameServerResourceRecord(Domain domain, Domain nsDomain) {
            Add(new NameServerResourceRecord(domain, nsDomain, ttl));
        }

        public void AddCanonicalNameResourceRecord(string domain, string cname) {
            AddCanonicalNameResourceRecord(new Domain(domain), new Domain(cname));
        }

        public void AddCanonicalNameResourceRecord(Domain domain, Domain cname) {
            Add(new CanonicalNameResourceRecord(domain, cname, ttl));
        }

        public void AddPointerResourceRecord(string ip, string pointer) {
            AddPointerResourceRecord(IPAddress.Parse(ip), new Domain(pointer));
        }

        public void AddPointerResourceRecord(IPAddress ip, Domain pointer) {
            Add(new PointerResourceRecord(ip, pointer, ttl));
        }

        public void AddMailExchangeResourceRecord(string domain, int preference, string exchange) {
            AddMailExchangeResourceRecord(new Domain(domain), preference, new Domain(exchange));
        }

        public void AddMailExchangeResourceRecord(Domain domain, int preference, Domain exchange) {
            Add(new MailExchangeResourceRecord(domain, preference, exchange));
        }

        public void AddTextResourceRecord(string domain, string attributeName, string attributeValue) {
            Add(new TextResourceRecord(new Domain(domain), attributeName, attributeValue, ttl));
        }
    }
}
