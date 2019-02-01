using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace DNS.Protocol.ResourceRecords
{
    public class SrvResourceRecord : BaseResourceRecord
    {
        private static IResourceRecord Create(
            Domain service,
            TimeSpan ttl,
            int priority,
            int weight,
            int port,
            Domain target
        )
        {
            var data = new List<byte>();

            data.AddRange(BitConverter.GetBytes((ushort) priority).Reverse());
            data.AddRange(BitConverter.GetBytes((ushort) weight).Reverse());
            data.AddRange(BitConverter.GetBytes((ushort) port).Reverse());
            data.AddRange(target.ToArray());

            var type = RecordType.SRV;


            return new ResourceRecord(service, data.ToArray(), type, RecordClass.IN, ttl);
        }

        public SrvResourceRecord(IResourceRecord record) : base(record)
        {
            //parse record
            Service = Name;
        }

        public SrvResourceRecord(
            Domain domain,
            int priority,
            int weight,
            int port,
            Domain target,
            TimeSpan ttl = default(TimeSpan)) :
            base(Create(domain, ttl, priority, weight, port, target))
        {
            Target = target;
            Service = Service;
            Weight = weight;
            Port = port;
            Target = target;
            Priority = priority;
            //create record
        }

        public Domain Target { get; set; }
        public Domain Service { get; set; }
        public int Priority { get; set; }
        public int Weight { get; set; }
        public int Port { get; set; }
    }
}