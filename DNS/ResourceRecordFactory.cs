using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;

namespace DNS.Protocol {
    public static class ResourceRecordFactory {
        public static IList<IResourceRecord> GetAllFromArray(byte[] message, int offset, int count) {
            return GetAllFromArray(message, offset, count, out offset);
        }

        public static IList<IResourceRecord> GetAllFromArray(byte[] message, int offset, int count, out int endOffset) { 
            IList<IResourceRecord> result = new List<IResourceRecord>();

            for (int i = 0; i < count; i++) {
                result.Add(FromArray(message, offset, out offset));
            }

            endOffset = offset;
            return result;
        }

        public static IResourceRecord FromArray(byte[] message, int offset) {
            return FromArray(message, offset, out offset);
        }

        public static IResourceRecord FromArray(byte[] message, int offset, out int endOffest) {
            ResourceRecord record = ResourceRecord.FromArray(message, offset, out endOffest);
            int dataOffset = endOffest - record.DataLength;

            switch(record.Type) {
                case RecordType.A: 
                case RecordType.AAAA:
                    return new IPAddressResourceRecord(record);
                case RecordType.NS: 
                    return new NameServerResourceRecord(record, message, dataOffset);
                case RecordType.CNAME: 
                    return new CanonicalNameResourceRecord(record, message, dataOffset);
                case RecordType.SOA:
                    return new StartOfAuthorityResourceRecord(record, message, dataOffset);
                case RecordType.PTR:
                    return new PointerResourceRecord(record, message, dataOffset);
                case RecordType.MX:
                    return new MailExchangeResourceRecord(record, message, dataOffset);
                default: 
                    return record;
            }
        }
    }

    public abstract class ResourceRecordBase : IResourceRecord {
        private IResourceRecord record;

        public ResourceRecordBase(IResourceRecord record) {
            this.record = record;
        }

        public Domain Name {
            get { return record.Name; }
        }

        public RecordType Type {
            get { return record.Type; }
        }

        public RecordClass Class {
            get { return record.Class; }
        }

        public TimeSpan TimeToLive {
            get { return record.TimeToLive; }
        }

        public int DataLength {
            get { return record.DataLength; }
        }

        public byte[] Data {
            get { return record.Data; }
        }

        public int Size {
            get { return record.Size;  }
        }

        public byte[] ToArray() {
            return record.ToArray();
        }

        internal Marshalling.Object Dump() {
            return Marshalling.Object.New(this)
                .Add("Name", "Type", "Class", "TimeToLive", "DataLength");
        }
    }

    public class IPAddressResourceRecord : ResourceRecordBase {
        public IPAddressResourceRecord(IResourceRecord record) : base(record) {
            IPAddress = new IPAddress(Data);
        }

        public IPAddress IPAddress {
            get;
            private set;
        }

        public override string ToString() {
            return Dump().Add("IPAddress").ToString();
        }
    }

    public class NameServerResourceRecord : ResourceRecordBase {
        public NameServerResourceRecord(IResourceRecord record, byte[] message, int dataOffset) : base(record) {
            NSDomainName = Domain.FromArray(message, dataOffset);
        }

        public Domain NSDomainName {
            get;
            private set;
        }

        public override string ToString() {
            return Dump().Add("NSDomainName").ToString();
        }
    }

    public class CanonicalNameResourceRecord : ResourceRecordBase {
        public CanonicalNameResourceRecord(IResourceRecord record, byte[] message, int dataOffset) : base(record) {
            CanonicalDomainName = Domain.FromArray(message, dataOffset);
        }

        public Domain CanonicalDomainName {
            get;
            private set;
        }

        public override string ToString() {
            return Dump().Add("CanonicalDomainName").ToString();
        }
    }

    public class PointerResourceRecord : ResourceRecordBase {
        public PointerResourceRecord(IResourceRecord record, byte[] message, int dataOffset) : base(record) {
            PointerDomainName = Domain.FromArray(message, dataOffset);
        }

        public Domain PointerDomainName {
            get;
            private set;
        }

        public override string ToString() {
            return Dump().Add("PointerDomainName").ToString();
        }
    }

    public class MailExchangeResourceRecord : ResourceRecordBase {
        private const int PREFERENCE_SIZE = 2;

        public MailExchangeResourceRecord(IResourceRecord record, byte[] message, int dataOffset) : base(record) { 
            byte[] preference = new byte[MailExchangeResourceRecord.PREFERENCE_SIZE];
            Array.Copy(message, dataOffset, preference, 0, preference.Length);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(preference);
            }

            dataOffset += MailExchangeResourceRecord.PREFERENCE_SIZE;

            Preference = BitConverter.ToUInt16(preference, 0);
            ExchangeDomainName = Domain.FromArray(message, dataOffset);
        }

        public int Preference {
            get;
            private set;
        }

        public Domain ExchangeDomainName {
            get;
            private set;
        }

        public override string ToString() {
            return Dump().Add("Preference", "ExchangeDomainName").ToString();
        }
    }

    public class StartOfAuthorityResourceRecord : ResourceRecordBase {
        public StartOfAuthorityResourceRecord(IResourceRecord record, byte[] message, int dataOffset) : base(record) {
            MasterDomainName = Domain.FromArray(message, dataOffset, out dataOffset);
            ResponsibleDomainName = Domain.FromArray(message, dataOffset, out dataOffset);

            Tail tail = Marshalling.Struct.GetStruct<Tail>(message, dataOffset, Tail.SIZE);

            SerialNumber = tail.SerialNumber;
            RefreshInterval = tail.RefreshInterval;
            RetryInterval = tail.RetryInterval;
            ExpireInterval = tail.ExpireInterval;
            MinimumTimeToLive = tail.MinimumTimeToLive;
        }

        public Domain MasterDomainName {
            get;
            private set;
        }

        public Domain ResponsibleDomainName {
            get;
            private set;
        }

        public long SerialNumber {
            get;
            private set;
        }

        public TimeSpan RefreshInterval {
            get;
            private set;
        }

        public TimeSpan RetryInterval {
            get;
            private set;
        }

        public TimeSpan ExpireInterval {
            get;
            private set;
        }

        public TimeSpan MinimumTimeToLive {
            get;
            private set;
        }

        [Marshalling.Endian(Marshalling.Endianness.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct Tail {
            public const int SIZE = 20;

            private uint serialNumber;
            private uint refreshInterval;
            private uint retryInterval;
            private uint expireInterval;
            private uint ttl;

            public long SerialNumber {
                get { return serialNumber; }
            }

            public TimeSpan RefreshInterval {
                get { return TimeSpan.FromSeconds(refreshInterval); }
            }

            public TimeSpan RetryInterval {
                get { return TimeSpan.FromSeconds(retryInterval); }
            }

            public TimeSpan ExpireInterval {
                get { return TimeSpan.FromSeconds(expireInterval); }
            }

            public TimeSpan MinimumTimeToLive {
                get { return TimeSpan.FromSeconds(ttl); }
            }
        }
    }
}
