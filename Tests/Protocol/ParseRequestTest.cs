using System;
using System.Collections.Generic;
using NUnit.Framework;
using DNS.Protocol;

namespace DNS.Tests.Protocol {
    [TestFixture]
    public class ParseRequestTest {
        [Test]
        public void BasicQuestionRequestWithEmptyHeader() {
            byte[] content = Helper.ReadFixture("Request", "empty-header_basic-question");
            Request request = Request.FromArray(content);

            Assert.AreEqual(0, request.Id);
            Assert.AreEqual(false, request.RecursionDesired);
            Assert.AreEqual(17, request.Size);
            Assert.AreEqual(1, request.Questions.Count);

            Question question = request.Questions[0];

            Assert.AreEqual(RecordType.A, question.Type);
            Assert.AreEqual(RecordClass.IN, question.Class);
            Assert.AreEqual("", question.Name.ToString());
        }

        [Test]
        public void SingleQuestionRequestWithHeader() {
            byte[] content = Helper.ReadFixture("Request", "id-rd_www.google.com-cname");

            Request request = Request.FromArray(content);

            Assert.AreEqual(1, request.Id);
            Assert.AreEqual(true, request.RecursionDesired);
            Assert.AreEqual(32, request.Size);
            Assert.AreEqual(1, request.Questions.Count);

            Question question = request.Questions[0];

            Assert.AreEqual(RecordType.CNAME, question.Type);
            Assert.AreEqual(RecordClass.IN, question.Class);
            Assert.AreEqual("www.google.com", question.Name.ToString());
        }

        [Test]
        public void RequestWithMultipleQuestions() {
            byte[] content = Helper.ReadFixture("Request", "multiple-questions");

            Request request = Request.FromArray(content);

            Assert.AreEqual(1, request.Id);
            Assert.AreEqual(true, request.RecursionDesired);
            Assert.AreEqual(43, request.Size);
            Assert.AreEqual(2, request.Questions.Count);

            Question question = request.Questions[0];

            Assert.AreEqual(RecordType.CNAME, question.Type);
            Assert.AreEqual(RecordClass.IN, question.Class);
            Assert.AreEqual("www.google.com", question.Name.ToString());

            question = request.Questions[1];

            Assert.AreEqual(RecordType.A, question.Type);
            Assert.AreEqual(RecordClass.ANY, question.Class);
            Assert.AreEqual("dr.dk", question.Name.ToString());
        }
    }
}
