using System;
using System.Collections.Generic;
using NUnit.Framework;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

namespace DNS.Tests.Protocol.ResourceRecords {
    [TestFixture]
    public class SerializeResponseTest {
        [Test]
        public void BasicQuestionResponseWithEmptyHeader() {
            Header header = new Header();
            header.Response = true;

            Domain domain = new Domain(Helper.GetArray<string>());
            Question question = new Question(domain, RecordType.A, RecordClass.IN);
            ResourceRecord record = new ResourceRecord(domain, Helper.GetArray<byte>(0, 0, 0, 0), 
                RecordType.A, RecordClass.IN, new TimeSpan());

            Response response = new Response(header,
                Helper.GetList(question), 
                Helper.GetList<IResourceRecord>(record),
                Helper.GetList<IResourceRecord>(record),
                Helper.GetList<IResourceRecord>(record));

            byte[] content = Helper.ReadFixture("Response", "empty-header_basic");

            CollectionAssert.AreEqual(content, response.ToArray());
        }

        [Test]
        public void RequestWithHeaderAndResourceRecords() {
            Header header = new Header();
            header.Response = true;

            Domain domain = new Domain(Helper.GetArray("www", "google", "com"));
            Question question = new Question(domain, RecordType.A, RecordClass.IN);

            Domain domain1 = new Domain(Helper.GetArray("www", "google", "com"));
            ResourceRecord record1 = new ResourceRecord(domain1, Helper.GetArray<byte>(3, 119, 119, 119, 0),
                RecordType.CNAME, RecordClass.IN, TimeSpan.FromSeconds(1));

            Domain domain2 = new Domain(Helper.GetArray("dr", "dk"));
            ResourceRecord record2 = new ResourceRecord(domain2, Helper.GetArray<byte>(1, 1, 1, 1),
                RecordType.A, RecordClass.ANY, TimeSpan.FromSeconds(0));

            Domain domain3 = new Domain(Helper.GetArray("www"));
            ResourceRecord record3 = new ResourceRecord(domain3, Helper.GetArray<byte>(192, 12),
                RecordType.CNAME, RecordClass.ANY, TimeSpan.FromSeconds(1));

            Response response = new Response(header,
                Helper.GetList(question),
                Helper.GetList<IResourceRecord>(record1),
                Helper.GetList<IResourceRecord>(record2),
                Helper.GetList<IResourceRecord>(record3));

            response.Id = 1;
            response.RecursionAvailable = true;

            byte[] content = Helper.ReadFixture("Response", "id-ra_all");

            CollectionAssert.AreEqual(content, response.ToArray());
        }
    }
}
