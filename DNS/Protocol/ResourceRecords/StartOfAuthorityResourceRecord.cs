using System;
using System.Runtime.InteropServices;
using DNS.Protocol.Utils;

namespace DNS.Protocol.ResourceRecords {
    public class StartOfAuthorityResourceRecord : BaseResourceRecord {
        private static IResourceRecord Create(Domain domain, Domain master, Domain responsible, long serial,
                TimeSpan refresh, TimeSpan retry, TimeSpan expire, TimeSpan minTtl, TimeSpan ttl) {
            ByteStream data = new ByteStream(Options.SIZE + master.Size + responsible.Size);
            Options tail = new Options() {
                SerialNumber = serial,
                RefreshInterval = refresh,
                RetryInterval = retry,
                ExpireInterval = expire,
                MinimumTimeToLive = minTtl
            };

            data
                .Append(master.ToArray())
                .Append(responsible.ToArray())
                .Append(Marshalling.Struct.GetBytes(tail));

            return new ResourceRecord(domain, data.ToArray(), RecordType.SOA, RecordClass.IN, ttl);
        }

        public StartOfAuthorityResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
            : base(record) {
            MasterDomainName = Domain.FromArray(message, dataOffset, out dataOffset);
            ResponsibleDomainName = Domain.FromArray(message, dataOffset, out dataOffset);

            Options tail = Marshalling.Struct.GetStruct<Options>(message, dataOffset, Options.SIZE);

            SerialNumber = tail.SerialNumber;
            RefreshInterval = tail.RefreshInterval;
            RetryInterval = tail.RetryInterval;
            ExpireInterval = tail.ExpireInterval;
            MinimumTimeToLive = tail.MinimumTimeToLive;
        }

        public StartOfAuthorityResourceRecord(Domain domain, Domain master, Domain responsible, long serial,
                TimeSpan refresh, TimeSpan retry, TimeSpan expire, TimeSpan minTtl, TimeSpan ttl = default(TimeSpan)) :
            base(Create(domain, master, responsible, serial, refresh, retry, expire, minTtl, ttl)) {
            MasterDomainName = master;
            ResponsibleDomainName = responsible;

            SerialNumber = serial;
            RefreshInterval = refresh;
            RetryInterval = retry;
            ExpireInterval = expire;
            MinimumTimeToLive = minTtl;
        }

        public StartOfAuthorityResourceRecord(Domain domain, Domain master, Domain responsible,
                Options options = default(Options), TimeSpan ttl = default(TimeSpan)) :
            this(domain, master, responsible, options.SerialNumber, options.RefreshInterval, options.RetryInterval,
                    options.ExpireInterval, options.MinimumTimeToLive, ttl) { }

        public Domain MasterDomainName { get; }
        public Domain ResponsibleDomainName { get; }
        public long SerialNumber { get; }
        public TimeSpan RefreshInterval { get; }
        public TimeSpan RetryInterval { get; }
        public TimeSpan ExpireInterval { get; }
        public TimeSpan MinimumTimeToLive { get; }

        public override string ToString() {
            return Stringify().Add("MasterDomainName", "ResponsibleDomainName", "SerialNumber").ToString();
        }

        [Marshalling.Endian(Marshalling.Endianness.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Options {
            public const int SIZE = 20;

            private uint serialNumber;
            private uint refreshInterval;
            private uint retryInterval;
            private uint expireInterval;
            private uint ttl;

            public long SerialNumber {
                get { return serialNumber; }
                set { serialNumber = (uint) value; }
            }

            public TimeSpan RefreshInterval {
                get { return TimeSpan.FromSeconds(refreshInterval); }
                set { refreshInterval = (uint) value.TotalSeconds; }
            }

            public TimeSpan RetryInterval {
                get { return TimeSpan.FromSeconds(retryInterval); }
                set { retryInterval = (uint) value.TotalSeconds; }
            }

            public TimeSpan ExpireInterval {
                get { return TimeSpan.FromSeconds(expireInterval); }
                set { expireInterval = (uint) value.TotalSeconds; }
            }

            public TimeSpan MinimumTimeToLive {
                get { return TimeSpan.FromSeconds(ttl); }
                set { ttl = (uint) value.TotalSeconds; }
            }
        }
    }
}
