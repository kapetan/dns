using System;
using DNS.Protocol.Utils;

namespace DNS.Protocol.ResourceRecords {
    public abstract class BaseResourceRecord : IResourceRecord {
        private readonly IResourceRecord record;

        public BaseResourceRecord(IResourceRecord record) {
            this.record = record;
        }

        public Domain Name => record.Name; 

        public RecordType Type => record.Type; 

        public RecordClass Class => record.Class; 

        public TimeSpan TimeToLive => record.TimeToLive;

        public int DataLength => record.DataLength;

        public byte[] Data => record.Data; 

        public int Size => record.Size;

        public byte[] ToArray() {
            return record.ToArray();
        }

        internal ObjectStringifier Stringify() {
            return ObjectStringifier.New(this)
                .Add("Name", "Type", "Class", "TimeToLive", "DataLength");
        }
    }
}
