using System;
using System.Collections.Generic;
using Xunit;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

namespace DNS.Tests.Protocol {
    
    public class ParseResponseTest {
        [Fact]
        public void BasicQuestionResponseWithEmptyHeader() {
            byte[] content = Helper.ReadFixture("Response", "empty-header_basic");
            Response response = Response.FromArray(content);

            Assert.Equal(0, response.Id);
            Assert.Equal(false, response.RecursionAvailable);
            Assert.Equal(62, response.Size);
            Assert.Equal(1, response.Questions.Count);
            Assert.Equal(1, response.AnswerRecords.Count);
            Assert.Equal(1, response.AuthorityRecords.Count);
            Assert.Equal(1, response.AdditionalRecords.Count);

            Question question = response.Questions[0];

            Assert.Equal(RecordType.A, question.Type);
            Assert.Equal(RecordClass.IN, question.Class);
            Assert.Equal("", question.Name.ToString());

            IResourceRecord record = response.AnswerRecords[0];

            Assert.Equal("", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(0, 0, 0, 0), record.Data);
            Assert.Equal(RecordType.A, record.Type);
            Assert.Equal(RecordClass.IN, record.Class);
            Assert.Equal(new TimeSpan(0), record.TimeToLive);

            record = response.AuthorityRecords[0];

            Assert.Equal("", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(0, 0, 0, 0), record.Data);
            Assert.Equal(RecordType.A, record.Type);
            Assert.Equal(RecordClass.IN, record.Class);
            Assert.Equal(new TimeSpan(0), record.TimeToLive);

            record = response.AdditionalRecords[0];

            Assert.Equal("", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(0, 0, 0, 0), record.Data);
            Assert.Equal(RecordType.A, record.Type);
            Assert.Equal(RecordClass.IN, record.Class);
            Assert.Equal(new TimeSpan(0), record.TimeToLive);
        }

        [Fact]
        public void RequestWithHeaderAndResourceRecords() {
            byte[] content = Helper.ReadFixture("Response", "id-ra_all");
            Response response = Response.FromArray(content);

            Assert.Equal(1, response.Id);
            Assert.Equal(true, response.RecursionAvailable);
            Assert.Equal(101, response.Size);
            Assert.Equal(1, response.Questions.Count);
            Assert.Equal(1, response.AnswerRecords.Count);
            Assert.Equal(1, response.AuthorityRecords.Count);
            Assert.Equal(1, response.AdditionalRecords.Count);

            Question question = response.Questions[0];

            Assert.Equal(RecordType.A, question.Type);
            Assert.Equal(RecordClass.IN, question.Class);
            Assert.Equal("www.google.com", question.Name.ToString());

            IResourceRecord record = response.AnswerRecords[0];

            Assert.Equal("www.google.com", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(3, 119, 119, 119, 0), record.Data);
            Assert.Equal(RecordType.CNAME, record.Type);
            Assert.Equal(RecordClass.IN, record.Class);
            Assert.Equal(TimeSpan.FromSeconds(1), record.TimeToLive);

            record = response.AuthorityRecords[0];

            Assert.Equal("dr.dk", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(1, 1, 1, 1), record.Data);
            Assert.Equal(RecordType.A, record.Type);
            Assert.Equal(RecordClass.ANY, record.Class);
            Assert.Equal(TimeSpan.FromSeconds(0), record.TimeToLive);

            record = response.AdditionalRecords[0];

            Assert.Equal("www", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(192, 12), record.Data);
            Assert.Equal(RecordType.CNAME, record.Type);
            Assert.Equal(RecordClass.ANY, record.Class);
            Assert.Equal(TimeSpan.FromSeconds(1), record.TimeToLive);
        }
    }
}
