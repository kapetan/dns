using System;
using System.Collections.Generic;
using NUnit.Framework;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

namespace DNS.Tests.Protocol.ResourceRecords {
    [TestFixture]
    public class ParseResourceRecordTest {
        [Test]
        public void BasicResourceRecordWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_basic");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.AreEqual("", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray<byte>(), record.Data);
            Assert.AreEqual(RecordType.A, record.Type);
            Assert.AreEqual(RecordClass.IN, record.Class);
            Assert.AreEqual(new TimeSpan(0), record.TimeToLive);
            Assert.AreEqual(11, record.Size);
            Assert.AreEqual(11, endOffset);
        }

        [Test]
        public void BasicResourceRecordWithMultipleLabelDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "www.google.com_basic");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.AreEqual("www.google.com", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray<byte>(), record.Data);
            Assert.AreEqual(RecordType.A, record.Type);
            Assert.AreEqual(RecordClass.IN, record.Class);
            Assert.AreEqual(new TimeSpan(0), record.TimeToLive);
            Assert.AreEqual(26, record.Size);
            Assert.AreEqual(26, endOffset);
        }

        [Test]
        public void DataResourceRecordWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_data");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.AreEqual("", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray(1, 1), record.Data);
            Assert.AreEqual(RecordType.A, record.Type);
            Assert.AreEqual(RecordClass.IN, record.Class);
            Assert.AreEqual(new TimeSpan(0), record.TimeToLive);
            Assert.AreEqual(13, record.Size);
            Assert.AreEqual(13, endOffset);
        }

        [Test]
        public void CNameResourceRecordWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_cname");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.AreEqual("", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray<byte>(), record.Data);
            Assert.AreEqual(RecordType.CNAME, record.Type);
            Assert.AreEqual(RecordClass.IN, record.Class);
            Assert.AreEqual(new TimeSpan(0), record.TimeToLive);
            Assert.AreEqual(11, record.Size);
            Assert.AreEqual(11, endOffset);
        }

        [Test]
        public void AnyResourceRecordWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_any");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.AreEqual("", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray<byte>(), record.Data);
            Assert.AreEqual(RecordType.A, record.Type);
            Assert.AreEqual(RecordClass.ANY, record.Class);
            Assert.AreEqual(new TimeSpan(0), record.TimeToLive);
            Assert.AreEqual(11, record.Size);
            Assert.AreEqual(11, endOffset);
        }

        [Test]
        public void TtlResourceRecordWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_ttl");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.AreEqual("", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray<byte>(), record.Data);
            Assert.AreEqual(RecordType.A, record.Type);
            Assert.AreEqual(RecordClass.IN, record.Class);
            Assert.AreEqual(TimeSpan.FromSeconds(1), record.TimeToLive);
            Assert.AreEqual(11, record.Size);
            Assert.AreEqual(11, endOffset);
        }

        [Test]
        public void AllSetResourceRecordWithMultipleLabelDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "www.google.com_all");

            ResourceRecord record = ResourceRecord.FromArray(content, 0, out endOffset);

            Assert.AreEqual("www.google.com", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray(1, 1), record.Data);
            Assert.AreEqual(RecordType.CNAME, record.Type);
            Assert.AreEqual(RecordClass.ANY, record.Class);
            Assert.AreEqual(TimeSpan.FromSeconds(1), record.TimeToLive);
            Assert.AreEqual(28, record.Size);
            Assert.AreEqual(28, endOffset);
        }

        [Test]
        public void MultipleResourceRecords() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("ResourceRecord", "multiple");
            IList<ResourceRecord> records = ResourceRecord.GetAllFromArray(content, 0, 3, out endOffset);

            Assert.AreEqual(3, records.Count);
            Assert.AreEqual(60, endOffset);

            ResourceRecord record = records[0];

            Assert.AreEqual("", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray<byte>(), record.Data);
            Assert.AreEqual(RecordType.A, record.Type);
            Assert.AreEqual(RecordClass.IN, record.Class);
            Assert.AreEqual(new TimeSpan(0), record.TimeToLive);
            Assert.AreEqual(11, record.Size);

            record = records[1];

            Assert.AreEqual("www.google.com", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray(1, 1), record.Data);
            Assert.AreEqual(RecordType.CNAME, record.Type);
            Assert.AreEqual(RecordClass.ANY, record.Class);
            Assert.AreEqual(TimeSpan.FromSeconds(1), record.TimeToLive);
            Assert.AreEqual(28, record.Size);

            record = records[2];

            Assert.AreEqual("dr.dk", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray(1, 1, 1, 1), record.Data);
            Assert.AreEqual(RecordType.A, record.Type);
            Assert.AreEqual(RecordClass.IN, record.Class);
            Assert.AreEqual(new TimeSpan(0), record.TimeToLive);
            Assert.AreEqual(21, record.Size);
        }
    }
}
