using Xunit;
using DNS.Protocol;

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
    }
}
