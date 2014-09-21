using System;
using System.Collections.Generic;
using NUnit.Framework;
using DNS.Protocol;

namespace DNS.Tests.Protocol {
    [TestFixture]
    public class ParseQuestionTest {
        [Test]
        public void BasicQuestionWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Question", "empty-domain_basic");
            Question question = Question.FromArray(content, 0, out endOffset);

            Assert.AreEqual("", question.Name.ToString());
            Assert.AreEqual(RecordType.A, question.Type);
            Assert.AreEqual(RecordClass.IN, question.Class);
            Assert.AreEqual(5, question.Size);
            Assert.AreEqual(5, endOffset);
        }

        [Test]
        public void BasicQuestionWithMultipleLabelDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Question", "www.google.com_basic");
            Question question = Question.FromArray(content, 0, out endOffset);

            Assert.AreEqual("www.google.com", question.Name.ToString());
            Assert.AreEqual(RecordType.A, question.Type);
            Assert.AreEqual(RecordClass.IN, question.Class);
            Assert.AreEqual(20, question.Size);
            Assert.AreEqual(20, endOffset);
        }

        [Test]
        public void CNameQuestionWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Question", "empty-domain_cname");
            Question question = Question.FromArray(content, 0, out endOffset);

            Assert.AreEqual("", question.Name.ToString());
            Assert.AreEqual(RecordType.CNAME, question.Type);
            Assert.AreEqual(RecordClass.IN, question.Class);
            Assert.AreEqual(5, question.Size);
            Assert.AreEqual(5, endOffset);
        }

        [Test]
        public void AnyQuestionWithEmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Question", "empty-domain_any");
            Question question = Question.FromArray(content, 0, out endOffset);

            Assert.AreEqual("", question.Name.ToString());
            Assert.AreEqual(RecordType.A, question.Type);
            Assert.AreEqual(RecordClass.ANY, question.Class);
            Assert.AreEqual(5, question.Size);
            Assert.AreEqual(5, endOffset);
        }

        [Test]
        public void AllSetQuestionWithMultipleLabelDomains() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Question", "www.google.com_all");
            Question question = Question.FromArray(content, 0, out endOffset);

            Assert.AreEqual("www.google.com", question.Name.ToString());
            Assert.AreEqual(RecordType.CNAME, question.Type);
            Assert.AreEqual(RecordClass.ANY, question.Class);
            Assert.AreEqual(20, question.Size);
            Assert.AreEqual(20, endOffset);
        }

        [Test]
        public void MultipleQuestions() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Question", "multiple");
            IList<Question> questions = Question.GetAllFromArray(content, 0, 3, out endOffset);

            Assert.AreEqual(3, questions.Count);
            Assert.AreEqual(36, endOffset);

            Question question = questions[0];

            Assert.AreEqual("", question.Name.ToString());
            Assert.AreEqual(RecordType.A, question.Type);
            Assert.AreEqual(RecordClass.IN, question.Class);
            Assert.AreEqual(5, question.Size);

            question = questions[1];

            Assert.AreEqual("www.google.com", question.Name.ToString());
            Assert.AreEqual(RecordType.CNAME, question.Type);
            Assert.AreEqual(RecordClass.IN, question.Class);
            Assert.AreEqual(20, question.Size);

            question = questions[2];

            Assert.AreEqual("dr.dk", question.Name.ToString());
            Assert.AreEqual(RecordType.A, question.Type);
            Assert.AreEqual(RecordClass.ANY, question.Class);
            Assert.AreEqual(11, question.Size);
        }
    }
}
