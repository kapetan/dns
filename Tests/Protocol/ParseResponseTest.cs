using System;
using System.Collections.Generic;
using NUnit.Framework;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

namespace DNS.Tests.Protocol {
    [TestFixture]
    public class ParseResponseTest {
        [Test]
        public void BasicQuestionResponseWithEmptyHeader() {
            byte[] content = Helper.ReadFixture("Response", "empty-header_basic");
            Response response = Response.FromArray(content);

            Assert.AreEqual(0, response.Id);
            Assert.AreEqual(false, response.RecursionAvailable);
            Assert.AreEqual(62, response.Size);
            Assert.AreEqual(1, response.Questions.Count);
            Assert.AreEqual(1, response.AnswerRecords.Count);
            Assert.AreEqual(1, response.AuthorityRecords.Count);
            Assert.AreEqual(1, response.AdditionalRecords.Count);

            Question question = response.Questions[0];

            Assert.AreEqual(RecordType.A, question.Type);
            Assert.AreEqual(RecordClass.IN, question.Class);
            Assert.AreEqual("", question.Name.ToString());

            IResourceRecord record = response.AnswerRecords[0];

            Assert.AreEqual("", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray<byte>(0, 0, 0, 0), record.Data);
            Assert.AreEqual(RecordType.A, record.Type);
            Assert.AreEqual(RecordClass.IN, record.Class);
            Assert.AreEqual(new TimeSpan(0), record.TimeToLive);

            record = response.AuthorityRecords[0];

            Assert.AreEqual("", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray<byte>(0, 0, 0, 0), record.Data);
            Assert.AreEqual(RecordType.A, record.Type);
            Assert.AreEqual(RecordClass.IN, record.Class);
            Assert.AreEqual(new TimeSpan(0), record.TimeToLive);

            record = response.AdditionalRecords[0];

            Assert.AreEqual("", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray<byte>(0, 0, 0, 0), record.Data);
            Assert.AreEqual(RecordType.A, record.Type);
            Assert.AreEqual(RecordClass.IN, record.Class);
            Assert.AreEqual(new TimeSpan(0), record.TimeToLive);
        }

        [Test]
        public void RequestWithHeaderAndResourceRecords() {
            byte[] content = Helper.ReadFixture("Response", "id-ra_all");
            Response response = Response.FromArray(content);

            Assert.AreEqual(1, response.Id);
            Assert.AreEqual(true, response.RecursionAvailable);
            Assert.AreEqual(101, response.Size);
            Assert.AreEqual(1, response.Questions.Count);
            Assert.AreEqual(1, response.AnswerRecords.Count);
            Assert.AreEqual(1, response.AuthorityRecords.Count);
            Assert.AreEqual(1, response.AdditionalRecords.Count);

            Question question = response.Questions[0];

            Assert.AreEqual(RecordType.A, question.Type);
            Assert.AreEqual(RecordClass.IN, question.Class);
            Assert.AreEqual("www.google.com", question.Name.ToString());

            IResourceRecord record = response.AnswerRecords[0];

            Assert.AreEqual("www.google.com", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray<byte>(3, 119, 119, 119, 0), record.Data);
            Assert.AreEqual(RecordType.CNAME, record.Type);
            Assert.AreEqual(RecordClass.IN, record.Class);
            Assert.AreEqual(TimeSpan.FromSeconds(1), record.TimeToLive);

            record = response.AuthorityRecords[0];

            Assert.AreEqual("dr.dk", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray<byte>(1, 1, 1, 1), record.Data);
            Assert.AreEqual(RecordType.A, record.Type);
            Assert.AreEqual(RecordClass.ANY, record.Class);
            Assert.AreEqual(TimeSpan.FromSeconds(0), record.TimeToLive);

            record = response.AdditionalRecords[0];

            Assert.AreEqual("www", record.Name.ToString());
            CollectionAssert.AreEqual(Helper.GetArray<byte>(192, 12), record.Data);
            Assert.AreEqual(RecordType.CNAME, record.Type);
            Assert.AreEqual(RecordClass.ANY, record.Class);
            Assert.AreEqual(TimeSpan.FromSeconds(1), record.TimeToLive);
        }
    }
}
