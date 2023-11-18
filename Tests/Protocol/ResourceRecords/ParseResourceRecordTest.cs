using System;
using System.Collections.Generic;
using Xunit;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System.Linq;

namespace DNS.Tests.Protocol.ResourceRecords {

    public class ParseResourceRecordTest {
        [Fact]
        public void BasicResourceRecordWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_basic");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.Equal("", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(), record.Data);
            Assert.Equal(RecordType.A, record.Type);
            Assert.Equal(RecordClass.IN, record.Class);
            Assert.Equal(new TimeSpan(0), record.TimeToLive);
            Assert.Equal(11, record.Size);
            Assert.Equal(11, endOffset);
        }

        [Fact]
        public void BasicResourceRecordWithMultipleLabelDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "www.google.com_basic");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.Equal("www.google.com", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(), record.Data);
            Assert.Equal(RecordType.A, record.Type);
            Assert.Equal(RecordClass.IN, record.Class);
            Assert.Equal(new TimeSpan(0), record.TimeToLive);
            Assert.Equal(26, record.Size);
            Assert.Equal(26, endOffset);
        }

        [Fact]
        public void DataResourceRecordWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_data");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.Equal("", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(1, 1), record.Data);
            Assert.Equal(RecordType.A, record.Type);
            Assert.Equal(RecordClass.IN, record.Class);
            Assert.Equal(new TimeSpan(0), record.TimeToLive);
            Assert.Equal(13, record.Size);
            Assert.Equal(13, endOffset);
        }

        [Fact]
        public void CNameResourceRecordWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_cname");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.Equal("", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(), record.Data);
            Assert.Equal(RecordType.CNAME, record.Type);
            Assert.Equal(RecordClass.IN, record.Class);
            Assert.Equal(new TimeSpan(0), record.TimeToLive);
            Assert.Equal(11, record.Size);
            Assert.Equal(11, endOffset);
        }

        [Fact]
        public void AnyResourceRecordWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_any");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.Equal("", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(), record.Data);
            Assert.Equal(RecordType.A, record.Type);
            Assert.Equal(RecordClass.ANY, record.Class);
            Assert.Equal(new TimeSpan(0), record.TimeToLive);
            Assert.Equal(11, record.Size);
            Assert.Equal(11, endOffset);
        }

        [Fact]
        public void TtlResourceRecordWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_ttl");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.Equal("", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(), record.Data);
            Assert.Equal(RecordType.A, record.Type);
            Assert.Equal(RecordClass.IN, record.Class);
            Assert.Equal(TimeSpan.FromSeconds(1), record.TimeToLive);
            Assert.Equal(11, record.Size);
            Assert.Equal(11, endOffset);
        }

        [Fact]
        public void AllSetResourceRecordWithMultipleLabelDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "www.google.com_all");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.Equal("www.google.com", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(1, 1), record.Data);
            Assert.Equal(RecordType.CNAME, record.Type);
            Assert.Equal(RecordClass.ANY, record.Class);
            Assert.Equal(TimeSpan.FromSeconds(1), record.TimeToLive);
            Assert.Equal(28, record.Size);
            Assert.Equal(28, endOffset);
        }

        [Fact]
        public void MultipleResourceRecords() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "multiple");
            IList<ResourceRecord> records = ResourceRecord.GetAllFromArray(content, 0, 3, out endOffset);

            Assert.Equal(3, records.Count);
            Assert.Equal(60, endOffset);

            ResourceRecord record = records[0];

            Assert.Equal("", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(), record.Data);
            Assert.Equal(RecordType.A, record.Type);
            Assert.Equal(RecordClass.IN, record.Class);
            Assert.Equal(new TimeSpan(0), record.TimeToLive);
            Assert.Equal(11, record.Size);

            record = records[1];

            Assert.Equal("www.google.com", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(1, 1), record.Data);
            Assert.Equal(RecordType.CNAME, record.Type);
            Assert.Equal(RecordClass.ANY, record.Class);
            Assert.Equal(TimeSpan.FromSeconds(1), record.TimeToLive);
            Assert.Equal(28, record.Size);

            record = records[2];

            Assert.Equal("dr.dk", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(1, 1, 1, 1), record.Data);
            Assert.Equal(RecordType.A, record.Type);
            Assert.Equal(RecordClass.IN, record.Class);
            Assert.Equal(new TimeSpan(0), record.TimeToLive);
            Assert.Equal(21, record.Size);
        }

        [Fact]
        public void SrvResourceRecordWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_srv");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.Equal("", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(0, 10, 0, 60, 31, 144, 7, 101, 120, 97, 109, 112, 108, 101, 3, 99, 111, 109, 0), record.Data);
            Assert.Equal(RecordType.SRV, record.Type);
            Assert.Equal(RecordClass.IN, record.Class);
            Assert.Equal(new TimeSpan(0), record.TimeToLive);
            Assert.Equal(30, record.Size);
            Assert.Equal(30, endOffset);

            ServiceResourceRecord srv = new ServiceResourceRecord(record, content, 11);

            Assert.Equal(10, srv.Priority);
            Assert.Equal(60, srv.Weight);
            Assert.Equal(8080, srv.Port);
            Assert.Equal("example.com", srv.Target.ToString());
        }

        [Fact]
        public void OptResourceRecordWithOptionPadding() {
            byte[] content = Helper.ReadFixture("ResourceRecord", "edns-option-padding");

            ResourceRecord record = ResourceRecord.FromArray(content, 0);

            Assert.Equal("", record.Name.ToString());
            Assert.Equal(RecordType.OPT, record.Type);
            Assert.Equal(31, record.Size);

            OptResourceRecord opt = new OptResourceRecord(record);

            Assert.Equal(512, opt.UdpPayloadSize);
            Assert.Equal(0, opt.ExtendedResponseCode);
            Assert.Equal(0, opt.Version);
            Assert.Equal(0, opt.Z);
            Assert.Equal(1, opt.Options.Count);
            Assert.Equal(12, opt.Options[0].Code);
            Assert.Equal(16, opt.Options[0].Length);
            Assert.True(opt.Options[0].Data.All(x => x == 0));
        }
    }
}
