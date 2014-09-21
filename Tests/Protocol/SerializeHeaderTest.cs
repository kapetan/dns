using System;
using NUnit.Framework;
using DNS.Protocol;

namespace DNS.Tests.Protocol {
    [TestFixture]
    public class SerializeHeaderTest {
        [Test]
        public void EmptyHeader() {
            Header header = new Header();
            byte[] content = Helper.ReadFixture("Header", "empty");

            CollectionAssert.AreEqual(content, header.ToArray());
        }

        [Test]
        public void HeaderWithId() {
            Header header = new Header();
            header.Id = 1;
            byte[] content = Helper.ReadFixture("Header", "id");

            CollectionAssert.AreEqual(content, header.ToArray());
        }

        [Test]
        public void HeaderWithQueryResponseFlag() {
            Header header = new Header();
            header.Response = true;
            byte[] content = Helper.ReadFixture("Header", "qr");

            CollectionAssert.AreEqual(content, header.ToArray());
        }

        [Test]
        public void HeaderWithOperationCode() {
            Header header = new Header();
            header.OperationCode = OperationCode.Status;
            byte[] content = Helper.ReadFixture("Header", "opcode");

            CollectionAssert.AreEqual(content, header.ToArray());
        }

        [Test]
        public void HeaderWithAuthorativeAnswerFlag() {
            Header header = new Header();
            header.AuthorativeServer = true;
            byte[] content = Helper.ReadFixture("Header", "aa");

            CollectionAssert.AreEqual(content, header.ToArray());
        }

        [Test]
        public void HeaderWithTruncatedFlag() {
            Header header = new Header();
            header.Truncated = true;
            byte[] content = Helper.ReadFixture("Header", "tc");

            CollectionAssert.AreEqual(content, header.ToArray());
        }

        [Test]
        public void HeaderWithRecusionDesiredFlag() {
            Header header = new Header();
            header.RecursionDesired = true;
            byte[] content = Helper.ReadFixture("Header", "rd");

            CollectionAssert.AreEqual(content, header.ToArray());
        }

        [Test]
        public void HeaderWithRecusionAvailableFlag() {
            Header header = new Header();
            header.RecursionAvailable = true;
            byte[] content = Helper.ReadFixture("Header", "ra");

            CollectionAssert.AreEqual(content, header.ToArray());
        }

        [Test]
        public void HeaderWithResponseCode() {
            Header header = new Header();
            header.ResponseCode = ResponseCode.ServerFailure;
            byte[] content = Helper.ReadFixture("Header", "rcode");

            CollectionAssert.AreEqual(content, header.ToArray());
        }

        [Test]
        public void HeaderWithQuestionCount() {
            Header header = new Header();
            header.QuestionCount = 1;
            byte[] content = Helper.ReadFixture("Header", "qdcount");

            CollectionAssert.AreEqual(content, header.ToArray());
        }

        [Test]
        public void HeaderWithAnswerRecordCount() {
            Header header = new Header();
            header.AnswerRecordCount = 1;
            byte[] content = Helper.ReadFixture("Header", "ancount");

            CollectionAssert.AreEqual(content, header.ToArray());
        }

        [Test]
        public void HeaderWithAuthorityRecordCount() {
            Header header = new Header();
            header.AuthorityRecordCount = 1;
            byte[] content = Helper.ReadFixture("Header", "nscount");

            CollectionAssert.AreEqual(content, header.ToArray());
        }

        [Test]
        public void HeaderWithAdditionalRecordCount() {
            Header header = new Header();
            header.AdditionalRecordCount = 1;
            byte[] content = Helper.ReadFixture("Header", "arcount");

            CollectionAssert.AreEqual(content, header.ToArray());
        }

        [Test]
        public void HeaderWithAllSet() {
            Header header = new Header();
            header.Id = 1;
            header.Response = true;
            header.OperationCode = OperationCode.Status;
            header.AuthorativeServer = true;
            header.Truncated = true;
            header.RecursionDesired = true;
            header.RecursionAvailable = true;
            header.ResponseCode = ResponseCode.ServerFailure;
            header.QuestionCount = 1;
            header.AnswerRecordCount = 1;
            header.AuthorityRecordCount = 1;
            header.AdditionalRecordCount = 1;

            byte[] content = Helper.ReadFixture("Header", "all");

            CollectionAssert.AreEqual(content, header.ToArray());
        }
    }
}
