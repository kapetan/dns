using System;
using Kapetan.Dns.Interface;

namespace Kapetan.Dns.Model
{
    public abstract class ResourceRecordBase : IResourceRecord
    {
        private IResourceRecord record;

        public ResourceRecordBase(IResourceRecord record)
        {
            this.record = record;
        }

        public Domain Name
        {
            get { return record.Name; }
        }

        public RecordType Type
        {
            get { return record.Type; }
        }

        public RecordClass Class
        {
            get { return record.Class; }
        }

        public TimeSpan TimeToLive
        {
            get { return record.TimeToLive; }
        }

        public int DataLength
        {
            get { return record.DataLength; }
        }

        public byte[] Data
        {
            get { return record.Data; }
        }

        public int Size
        {
            get { return record.Size; }
        }

        public byte[] ToArray()
        {
            return record.ToArray();
        }

        internal Marshalling.Object Dump()
        {
            return Marshalling.Object.New(this)
                .Add("Name", "Type", "Class", "TimeToLive", "DataLength");
        }
    }
}