using System;
using Xunit;
using DNS.Protocol;

namespace DNS.Tests.Protocol {
    
    public class SerializeQuestionTest {
        [Fact]
        public void BasicQuestionWithEmptyDomain() {
            byte[] content = Helper.ReadFixture("Question", "empty-domain_basic");
            Domain domain = new Domain(Helper.GetArray<string>());
            Question question = new Question(domain, RecordType.A, RecordClass.IN);

            Assert.Equal(content, question.ToArray());
        }

        [Fact]
        public void BasicQuestionWithMultipleLabelDomain() {
            byte[] content = Helper.ReadFixture("Question", "www.google.com_basic");
            Domain domain = new Domain(Helper.GetArray("www", "google", "com"));
            Question question = new Question(domain, RecordType.A, RecordClass.IN);

            Assert.Equal(content, question.ToArray());
        }

        [Fact]
        public void CNameQuestionWithEmptyDomain() {
            byte[] content = Helper.ReadFixture("Question", "empty-domain_any");
            Domain domain = new Domain(Helper.GetArray<string>());
            Question question = new Question(domain, RecordType.A, RecordClass.ANY);

            Assert.Equal(content, question.ToArray());
        }

        [Fact]
        public void AnyQuestionWithEmptyDomain() {
            byte[] content = Helper.ReadFixture("Question", "empty-domain_cname");
            Domain domain = new Domain(Helper.GetArray<string>());
            Question question = new Question(domain, RecordType.CNAME, RecordClass.IN);

            Assert.Equal(content, question.ToArray());
        }

        [Fact]
        public void AllSetQuestionWithMultipleLabelDomain() {
            byte[] content = Helper.ReadFixture("Question", "www.google.com_all");
            Domain domain = new Domain(Helper.GetArray("www", "google", "com"));
            Question question = new Question(domain, RecordType.CNAME, RecordClass.ANY);

            Assert.Equal(content, question.ToArray());
        }
    }
}
