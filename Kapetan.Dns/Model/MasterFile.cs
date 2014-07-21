﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Kapetan.Dns.Interface;

namespace Kapetan.Dns.Model
{
    public class MasterFile
    {
        private static readonly TimeSpan DEFAULT_TTL = new TimeSpan(0);

        private IList<IResourceRecord> entries = new List<IResourceRecord>();
        private TimeSpan ttl = DEFAULT_TTL;

        public MasterFile(TimeSpan ttl)
        {
            this.ttl = ttl;
        }

        public MasterFile() { }

        public void Add(IResourceRecord entry)
        {
            entries.Add(entry);
        }

        public void AddIPAddressResourceRecord(string domain, string ip)
        {
            AddIPAddressResourceRecord(new Domain(domain), IPAddress.Parse(ip));
        }

        public void AddIPAddressResourceRecord(Domain domain, IPAddress ip)
        {
            Add(new IPAddressResourceRecord(domain, ip, ttl));
        }

        public void AddNameServerResourceRecord(string domain, string nsDomain)
        {
            AddNameServerResourceRecord(new Domain(domain), new Domain(nsDomain));
        }

        public void AddNameServerResourceRecord(Domain domain, Domain nsDomain)
        {
            Add(new NameServerResourceRecord(domain, nsDomain, ttl));
        }

        public void AddCanonicalNameResourceRecord(string domain, string cname)
        {
            AddCanonicalNameResourceRecord(new Domain(domain), new Domain(cname));
        }

        public void AddCanonicalNameResourceRecord(Domain domain, Domain cname)
        {
            Add(new CanonicalNameResourceRecord(domain, cname, ttl));
        }

        public void AddPointerResourceRecord(string domain, string pointer)
        {
            AddPointerResourceRecord(new Domain(domain), new Domain(pointer));
        }

        public void AddPointerResourceRecord(Domain domain, Domain pointer)
        {
            Add(new PointerResourceRecord(domain, pointer, ttl));
        }

        public void AddMailExchangeResourceRecord(string domain, int preference, string exchange)
        {
            AddMailExchangeResourceRecord(new Domain(domain), preference, new Domain(exchange));
        }

        public void AddMailExchangeResourceRecord(Domain domain, int preference, Domain exchange)
        {
            Add(new MailExchangeResourceRecord(domain, preference, exchange));
        }

        public IList<IResourceRecord> Get(Domain domain, RecordType type)
        {
            return entries.Where(e => e.Name.Equals(domain) && e.Type == type).ToList();
        }

        public IList<IResourceRecord> Get(Question question)
        {
            return Get(question.Name, question.Type);
        }
    }
}
