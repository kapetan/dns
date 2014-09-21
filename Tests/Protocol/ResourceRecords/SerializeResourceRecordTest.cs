using System;
using NUnit.Framework;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

namespace DNS.Tests.Protocol.ResourceRecords {
    [TestFixture]
    public class SerializeResourceRecordTest {
        [Test]
        public void BasicResourceRecordWithEmptyDomain() {
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_basic");

            Domain domain = new Domain(Helper.GetArray<string>());
            ResourceRecord record = new ResourceRecord(domain, Helper.GetArray<byte>(), 
                RecordType.A, RecordClass.IN, new TimeSpan(0));

            CollectionAssert.AreEqual(content, record.ToArray());
        }

        [Test]
        public void BasicResourceRecordWithMultipleLabelDomain() {
            byte[] content = Helper.ReadFixture("ResourceRecord", "www.google.com_basic");

            Domain domain = new Domain(Helper.GetArray("www", "google", "com"));
            ResourceRecord record = new ResourceRecord(domain, Helper.GetArray<byte>(),
                RecordType.A, RecordClass.IN, new TimeSpan(0));

            CollectionAssert.AreEqual(content, record.ToArray());
        }

        [Test]
        public void DataResourceRecordWithEmptyDomain() {
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_data");

            byte[] data = Helper.GetArray<byte>(1, 1);
            Domain domain = new Domain(Helper.GetArray<string>());
            ResourceRecord record = new ResourceRecord(domain, data,
                RecordType.A, RecordClass.IN, new TimeSpan());

            CollectionAssert.AreEqual(content, record.ToArray());
        }

        [Test]
        public void CNameResourceRecordWithEmptyDomain() {
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_cname");

            Domain domain = new Domain(Helper.GetArray<string>());
            ResourceRecord record = new ResourceRecord(domain, Helper.GetArray<byte>(),
                RecordType.CNAME, RecordClass.IN, new TimeSpan(0));

            CollectionAssert.AreEqual(content, record.ToArray());
        }

        [Test]
        public void AnyResourceRecordWithEmptyDomain() {
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_any");

            Domain domain = new Domain(Helper.GetArray<string>());
            ResourceRecord record = new ResourceRecord(domain, Helper.GetArray<byte>(),
                RecordType.A, RecordClass.ANY, new TimeSpan(0));

            CollectionAssert.AreEqual(content, record.ToArray());
        }

        [Test]
        public void TtlResourceRecordWithEmptyDomain() {
            byte[] content = Helper.ReadFixture("ResourceRecord", "empty-domain_ttl");

            Domain domain = new Domain(Helper.GetArray<string>());
            ResourceRecord record = new ResourceRecord(domain, Helper.GetArray<byte>(),
                RecordType.A, RecordClass.IN, TimeSpan.FromSeconds(1));

            CollectionAssert.AreEqual(content, record.ToArray());
        }

        [Test]
        public void AllSetResourceRecordWithMultipleLabelDomain() {
            byte[] content = Helper.ReadFixture("ResourceRecord", "www.google.com_all");

            byte[] data = Helper.GetArray<byte>(1, 1);
            Domain domain = new Domain(Helper.GetArray("www", "google", "com"));
            ResourceRecord record = new ResourceRecord(domain, data,
                RecordType.CNAME, RecordClass.ANY, TimeSpan.FromSeconds(1));

            CollectionAssert.AreEqual(content, record.ToArray());
        }
    }
}
