using System;
using Xunit;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

namespace DNS.Tests.Protocol {

    public class ParseRequestTest {
        [Fact]
        public void BasicQuestionRequestWithEmptyHeader() {
            byte[] content = Helper.ReadFixture("Request", "empty-header_basic-question");
            Request request = Request.FromArray(content);

            Assert.Equal(0, request.Id);
            Assert.Equal(false, request.RecursionDesired);
            Assert.Equal(17, request.Size);
            Assert.Equal(1, request.Questions.Count);

            Question question = request.Questions[0];

            Assert.Equal(RecordType.A, question.Type);
            Assert.Equal(RecordClass.IN, question.Class);
            Assert.Equal("", question.Name.ToString());
        }

        [Fact]
        public void SingleQuestionRequestWithHeader() {
            byte[] content = Helper.ReadFixture("Request", "id-rd_www.google.com-cname");

            Request request = Request.FromArray(content);

            Assert.Equal(1, request.Id);
            Assert.Equal(true, request.RecursionDesired);
            Assert.Equal(32, request.Size);
            Assert.Equal(1, request.Questions.Count);

            Question question = request.Questions[0];

            Assert.Equal(RecordType.CNAME, question.Type);
            Assert.Equal(RecordClass.IN, question.Class);
            Assert.Equal("www.google.com", question.Name.ToString());
        }

        [Fact]
        public void RequestWithMultipleQuestions() {
            byte[] content = Helper.ReadFixture("Request", "multiple-questions");

            Request request = Request.FromArray(content);

            Assert.Equal(1, request.Id);
            Assert.Equal(true, request.RecursionDesired);
            Assert.Equal(43, request.Size);
            Assert.Equal(2, request.Questions.Count);

            Question question = request.Questions[0];

            Assert.Equal(RecordType.CNAME, question.Type);
            Assert.Equal(RecordClass.IN, question.Class);
            Assert.Equal("www.google.com", question.Name.ToString());

            question = request.Questions[1];

            Assert.Equal(RecordType.A, question.Type);
            Assert.Equal(RecordClass.ANY, question.Class);
            Assert.Equal("dr.dk", question.Name.ToString());
        }

        [Fact]
        public void RequestWithAdditionalRecords() {
            byte[] content = Helper.ReadFixture("Request", "edns-test");

            Request request = Request.FromArray(content);

            Assert.Equal(11772, request.Id);
            Assert.Equal(true, request.RecursionDesired);
            Assert.Equal(39, request.Size);
            Assert.Equal(1, request.Questions.Count);
            Assert.Equal(1, request.AdditionalRecords.Count);

            Question question = request.Questions[0];

            Assert.Equal(RecordType.A, question.Type);
            Assert.Equal(RecordClass.IN, question.Class);
            Assert.Equal("google.com", question.Name.ToString());

            IResourceRecord record = request.AdditionalRecords[0];

            Assert.Equal("", record.Name.ToString());
            Assert.Equal(Helper.GetArray<byte>(), record.Data);
            Assert.Equal(RecordType.OPT, record.Type);
            Assert.Equal(4096, (int) record.Class);
            Assert.Equal(TimeSpan.FromSeconds(0), record.TimeToLive);
        }
    }
}
