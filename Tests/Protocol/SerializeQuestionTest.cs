using System;
using NUnit.Framework;
using DNS.Protocol;

namespace DNS.Tests.Protocol {
    [TestFixture]
    public class SerializeQuestionTest {
        [Test]
        public void BasicQuestionWithEmptyDomain() {
            byte[] content = Helper.ReadFixture("Question", "empty-domain_basic");
            Domain domain = new Domain(Helper.GetArray<string>());
            Question question = new Question(domain, RecordType.A, RecordClass.IN);

            CollectionAssert.AreEqual(content, question.ToArray());
        }

        [Test]
        public void BasicQuestionWithMultipleLabelDomain() {
            byte[] content = Helper.ReadFixture("Question", "www.google.com_basic");
            Domain domain = new Domain(Helper.GetArray("www", "google", "com"));
            Question question = new Question(domain, RecordType.A, RecordClass.IN);

            CollectionAssert.AreEqual(content, question.ToArray());
        }

        [Test]
        public void CNameQuestionWithEmptyDomain() {
            byte[] content = Helper.ReadFixture("Question", "empty-domain_any");
            Domain domain = new Domain(Helper.GetArray<string>());
            Question question = new Question(domain, RecordType.A, RecordClass.ANY);

            CollectionAssert.AreEqual(content, question.ToArray());
        }

        [Test]
        public void AnyQuestionWithEmptyDomain() {
            byte[] content = Helper.ReadFixture("Question", "empty-domain_cname");
            Domain domain = new Domain(Helper.GetArray<string>());
            Question question = new Question(domain, RecordType.CNAME, RecordClass.IN);

            CollectionAssert.AreEqual(content, question.ToArray());
        }

        [Test]
        public void AllSetQuestionWithMultipleLabelDomain() {
            byte[] content = Helper.ReadFixture("Question", "www.google.com_all");
            Domain domain = new Domain(Helper.GetArray("www", "google", "com"));
            Question question = new Question(domain, RecordType.CNAME, RecordClass.ANY);

            CollectionAssert.AreEqual(content, question.ToArray());
        }
    }
}
