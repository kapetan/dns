using System.Collections.Generic;
using Xunit;
using DNS.Protocol;

namespace DNS.Tests.Protocol {
    
    public class ParseQuestionTest {
        [Fact]
        public void BasicQuestionWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Question", "empty-domain_basic");
            Question question = Question.FromArray(content, 0, out endOffset);

            Assert.Equal("", question.Name.ToString());
            Assert.Equal(RecordType.A, question.Type);
            Assert.Equal(RecordClass.IN, question.Class);
            Assert.Equal(5, question.Size);
            Assert.Equal(5, endOffset);
        }

        [Fact]
        public void BasicQuestionWithMultipleLabelDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Question", "www.google.com_basic");
            Question question = Question.FromArray(content, 0, out endOffset);

            Assert.Equal("www.google.com", question.Name.ToString());
            Assert.Equal(RecordType.A, question.Type);
            Assert.Equal(RecordClass.IN, question.Class);
            Assert.Equal(20, question.Size);
            Assert.Equal(20, endOffset);
        }

        [Fact]
        public void CNameQuestionWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Question", "empty-domain_cname");
            Question question = Question.FromArray(content, 0, out endOffset);

            Assert.Equal("", question.Name.ToString());
            Assert.Equal(RecordType.CNAME, question.Type);
            Assert.Equal(RecordClass.IN, question.Class);
            Assert.Equal(5, question.Size);
            Assert.Equal(5, endOffset);
        }

        [Fact]
        public void AnyQuestionWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Question", "empty-domain_any");
            Question question = Question.FromArray(content, 0, out endOffset);

            Assert.Equal("", question.Name.ToString());
            Assert.Equal(RecordType.A, question.Type);
            Assert.Equal(RecordClass.ANY, question.Class);
            Assert.Equal(5, question.Size);
            Assert.Equal(5, endOffset);
        }

        [Fact]
        public void AllSetQuestionWithMultipleLabelDomains() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Question", "www.google.com_all");
            Question question = Question.FromArray(content, 0, out endOffset);

            Assert.Equal("www.google.com", question.Name.ToString());
            Assert.Equal(RecordType.CNAME, question.Type);
            Assert.Equal(RecordClass.ANY, question.Class);
            Assert.Equal(20, question.Size);
            Assert.Equal(20, endOffset);
        }

        [Fact]
        public void MultipleQuestions() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Question", "multiple");
            IList<Question> questions = Question.GetAllFromArray(content, 0, 3, out endOffset);

            Assert.Equal(3, questions.Count);
            Assert.Equal(36, endOffset);

            Question question = questions[0];

            Assert.Equal("", question.Name.ToString());
            Assert.Equal(RecordType.A, question.Type);
            Assert.Equal(RecordClass.IN, question.Class);
            Assert.Equal(5, question.Size);

            question = questions[1];

            Assert.Equal("www.google.com", question.Name.ToString());
            Assert.Equal(RecordType.CNAME, question.Type);
            Assert.Equal(RecordClass.IN, question.Class);
            Assert.Equal(20, question.Size);

            question = questions[2];

            Assert.Equal("dr.dk", question.Name.ToString());
            Assert.Equal(RecordType.A, question.Type);
            Assert.Equal(RecordClass.ANY, question.Class);
            Assert.Equal(11, question.Size);
        }
    }
}
