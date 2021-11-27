using System;
using System.Runtime.InteropServices;

namespace DNS.Protocol.ResourceRecords {
    public class ServiceResourceRecord : BaseResourceRecord {
        private static IResourceRecord Create(Domain domain, ushort priority, ushort weight, ushort port, Domain target, TimeSpan ttl) {
            byte[] trg = target.ToArray();
            byte[] data = new byte[Head.SIZE + trg.Length];

            Head head = new Head() {
                Priority = priority,
                Weight = weight,
                Port = port
            };

            Marshalling.Struct.GetBytes(head).CopyTo(data, 0);
            trg.CopyTo(data, Head.SIZE);

            return new ResourceRecord(domain, data, RecordType.SRV, RecordClass.IN, ttl);
        }

        public ServiceResourceRecord(IResourceRecord record, byte[] message, int dataOffset) : base(record) {
            Head head = Marshalling.Struct.GetStruct<Head>(message, dataOffset, Head.SIZE);

            Priority = head.Priority;
            Weight = head.Weight;
            Port = head.Port;
            Target = Domain.FromArray(message, dataOffset + Head.SIZE);
        }

        public ServiceResourceRecord(Domain domain, ushort priority, ushort weight, ushort port, Domain target, TimeSpan ttl = default(TimeSpan)) :
                base(Create(domain, priority, weight, port, target, ttl)) {
            Priority = priority;
            Weight = weight;
            Port = port;
            Target = target;
        }

        public ushort Priority { get; }
        public ushort Weight { get; }
        public ushort Port { get; }
        public Domain Target { get; }

        public override string ToString() {
            return Stringify().Add("Priority", "Weight", "Port", "Target").ToString();
        }

        [Marshalling.Endian(Marshalling.Endianness.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct Head {
            public const int SIZE = 6;

            private ushort priority;
            private ushort weight;
            private ushort port;

            public ushort Priority {
                get { return priority; }
                set { priority = value; }
            }

            public ushort Weight {
                get { return weight; }
                set { weight = value; }
            }

            public ushort Port {
                get { return port; }
                set { port = value; }
            }
        }
    }
}
