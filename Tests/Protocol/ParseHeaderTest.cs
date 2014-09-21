using System;
using NUnit.Framework;
using DNS.Protocol;

namespace DNS.Tests.Protocol {
    [TestFixture]
    public class ParseHeaderTest {
        [Test]
        public void EmptyHeader() {
            byte[] content = Helper.ReadFixture("Header", "empty");
            Header header = Header.FromArray(content);

            Assert.AreEqual(0, header.Id);
            Assert.AreEqual(false, header.Response);
            Assert.AreEqual(OperationCode.Query, header.OperationCode);
            Assert.AreEqual(false, header.AuthorativeServer);
            Assert.AreEqual(false, header.Truncated);
            Assert.AreEqual(false, header.RecursionDesired);
            Assert.AreEqual(false, header.RecursionAvailable);
            Assert.AreEqual(ResponseCode.NoError, header.ResponseCode);
            Assert.AreEqual(0, header.QuestionCount);
            Assert.AreEqual(0, header.AnswerRecordCount);
            Assert.AreEqual(0, header.AuthorityRecordCount);
            Assert.AreEqual(0, header.AdditionalRecordCount);
        }

        [Test]
        public void HeaderWithId() {
            byte[] content = Helper.ReadFixture("Header", "id");
            Header header = Header.FromArray(content);

            Assert.AreEqual(1, header.Id);

            Assert.AreEqual(false, header.Response);
            Assert.AreEqual(OperationCode.Query, header.OperationCode);
            Assert.AreEqual(false, header.AuthorativeServer);
            Assert.AreEqual(false, header.Truncated);
            Assert.AreEqual(false, header.RecursionDesired);
            Assert.AreEqual(false, header.RecursionAvailable);
            Assert.AreEqual(ResponseCode.NoError, header.ResponseCode);
            Assert.AreEqual(0, header.QuestionCount);
            Assert.AreEqual(0, header.AnswerRecordCount);
            Assert.AreEqual(0, header.AuthorityRecordCount);
            Assert.AreEqual(0, header.AdditionalRecordCount);
        }

        [Test]
        public void HeaderWithQueryResponseFlag() {
            byte[] content = Helper.ReadFixture("Header", "qr");
            Header header = Header.FromArray(content);

            Assert.AreEqual(0, header.Id);

            Assert.AreEqual(true, header.Response);

            Assert.AreEqual(OperationCode.Query, header.OperationCode);
            Assert.AreEqual(false, header.AuthorativeServer);
            Assert.AreEqual(false, header.Truncated);
            Assert.AreEqual(false, header.RecursionDesired);
            Assert.AreEqual(false, header.RecursionAvailable);
            Assert.AreEqual(ResponseCode.NoError, header.ResponseCode);
            Assert.AreEqual(0, header.QuestionCount);
            Assert.AreEqual(0, header.AnswerRecordCount);
            Assert.AreEqual(0, header.AuthorityRecordCount);
            Assert.AreEqual(0, header.AdditionalRecordCount);
        }

        [Test]
        public void HeaderWithOperationCode() {
            byte[] content = Helper.ReadFixture("Header", "opcode");
            Header header = Header.FromArray(content);

            Assert.AreEqual(0, header.Id);
            Assert.AreEqual(false, header.Response);

            Assert.AreEqual(OperationCode.Status, header.OperationCode);

            Assert.AreEqual(false, header.AuthorativeServer);
            Assert.AreEqual(false, header.Truncated);
            Assert.AreEqual(false, header.RecursionDesired);
            Assert.AreEqual(false, header.RecursionAvailable);
            Assert.AreEqual(ResponseCode.NoError, header.ResponseCode);
            Assert.AreEqual(0, header.QuestionCount);
            Assert.AreEqual(0, header.AnswerRecordCount);
            Assert.AreEqual(0, header.AuthorityRecordCount);
            Assert.AreEqual(0, header.AdditionalRecordCount);
        }

        [Test]
        public void HeaderWithAuthorativeAnswerFlag() {
            byte[] content = Helper.ReadFixture("Header", "aa");
            Header header = Header.FromArray(content);

            Assert.AreEqual(0, header.Id);
            Assert.AreEqual(false, header.Response);
            Assert.AreEqual(OperationCode.Query, header.OperationCode);

            Assert.AreEqual(true, header.AuthorativeServer);

            Assert.AreEqual(false, header.Truncated);
            Assert.AreEqual(false, header.RecursionDesired);
            Assert.AreEqual(false, header.RecursionAvailable);
            Assert.AreEqual(ResponseCode.NoError, header.ResponseCode);
            Assert.AreEqual(0, header.QuestionCount);
            Assert.AreEqual(0, header.AnswerRecordCount);
            Assert.AreEqual(0, header.AuthorityRecordCount);
            Assert.AreEqual(0, header.AdditionalRecordCount);
        }

        [Test]
        public void HeaderWithTruncatedFlag() {
            byte[] content = Helper.ReadFixture("Header", "tc");
            Header header = Header.FromArray(content);

            Assert.AreEqual(0, header.Id);
            Assert.AreEqual(false, header.Response);
            Assert.AreEqual(OperationCode.Query, header.OperationCode);
            Assert.AreEqual(false, header.AuthorativeServer);

            Assert.AreEqual(true, header.Truncated);

            Assert.AreEqual(false, header.RecursionDesired);
            Assert.AreEqual(false, header.RecursionAvailable);
            Assert.AreEqual(ResponseCode.NoError, header.ResponseCode);
            Assert.AreEqual(0, header.QuestionCount);
            Assert.AreEqual(0, header.AnswerRecordCount);
            Assert.AreEqual(0, header.AuthorityRecordCount);
            Assert.AreEqual(0, header.AdditionalRecordCount);
        }

        [Test]
        public void HeaderWithRecusionDesiredFlag() {
            byte[] content = Helper.ReadFixture("Header", "rd");
            Header header = Header.FromArray(content);

            Assert.AreEqual(0, header.Id);
            Assert.AreEqual(false, header.Response);
            Assert.AreEqual(OperationCode.Query, header.OperationCode);
            Assert.AreEqual(false, header.AuthorativeServer);
            Assert.AreEqual(false, header.Truncated);

            Assert.AreEqual(true, header.RecursionDesired);

            Assert.AreEqual(false, header.RecursionAvailable);
            Assert.AreEqual(ResponseCode.NoError, header.ResponseCode);
            Assert.AreEqual(0, header.QuestionCount);
            Assert.AreEqual(0, header.AnswerRecordCount);
            Assert.AreEqual(0, header.AuthorityRecordCount);
            Assert.AreEqual(0, header.AdditionalRecordCount);
        }

        [Test]
        public void HeaderWithRecusionAvailableFlag() {
            byte[] content = Helper.ReadFixture("Header", "ra");
            Header header = Header.FromArray(content);

            Assert.AreEqual(0, header.Id);
            Assert.AreEqual(false, header.Response);
            Assert.AreEqual(OperationCode.Query, header.OperationCode);
            Assert.AreEqual(false, header.AuthorativeServer);
            Assert.AreEqual(false, header.Truncated);
            Assert.AreEqual(false, header.RecursionDesired);

            Assert.AreEqual(true, header.RecursionAvailable);

            Assert.AreEqual(ResponseCode.NoError, header.ResponseCode);
            Assert.AreEqual(0, header.QuestionCount);
            Assert.AreEqual(0, header.AnswerRecordCount);
            Assert.AreEqual(0, header.AuthorityRecordCount);
            Assert.AreEqual(0, header.AdditionalRecordCount);
        }

        [Test]
        public void HeaderWithResponseCode() {
            byte[] content = Helper.ReadFixture("Header", "rcode");
            Header header = Header.FromArray(content);

            Assert.AreEqual(0, header.Id);
            Assert.AreEqual(false, header.Response);
            Assert.AreEqual(OperationCode.Query, header.OperationCode);
            Assert.AreEqual(false, header.AuthorativeServer);
            Assert.AreEqual(false, header.Truncated);
            Assert.AreEqual(false, header.RecursionDesired);
            Assert.AreEqual(false, header.RecursionAvailable);

            Assert.AreEqual(ResponseCode.ServerFailure, header.ResponseCode);

            Assert.AreEqual(0, header.QuestionCount);
            Assert.AreEqual(0, header.AnswerRecordCount);
            Assert.AreEqual(0, header.AuthorityRecordCount);
            Assert.AreEqual(0, header.AdditionalRecordCount);
        }

        [Test]
        public void HeaderWithQuestionCount() {
            byte[] content = Helper.ReadFixture("Header", "qdcount");
            Header header = Header.FromArray(content);

            Assert.AreEqual(0, header.Id);
            Assert.AreEqual(false, header.Response);
            Assert.AreEqual(OperationCode.Query, header.OperationCode);
            Assert.AreEqual(false, header.AuthorativeServer);
            Assert.AreEqual(false, header.Truncated);
            Assert.AreEqual(false, header.RecursionDesired);
            Assert.AreEqual(false, header.RecursionAvailable);
            Assert.AreEqual(ResponseCode.NoError, header.ResponseCode);

            Assert.AreEqual(1, header.QuestionCount);

            Assert.AreEqual(0, header.AnswerRecordCount);
            Assert.AreEqual(0, header.AuthorityRecordCount);
            Assert.AreEqual(0, header.AdditionalRecordCount);
        }

        [Test]
        public void HeaderWithAnswerRecordCount() {
            byte[] content = Helper.ReadFixture("Header", "ancount");
            Header header = Header.FromArray(content);

            Assert.AreEqual(0, header.Id);
            Assert.AreEqual(false, header.Response);
            Assert.AreEqual(OperationCode.Query, header.OperationCode);
            Assert.AreEqual(false, header.AuthorativeServer);
            Assert.AreEqual(false, header.Truncated);
            Assert.AreEqual(false, header.RecursionDesired);
            Assert.AreEqual(false, header.RecursionAvailable);
            Assert.AreEqual(ResponseCode.NoError, header.ResponseCode);
            Assert.AreEqual(0, header.QuestionCount);

            Assert.AreEqual(1, header.AnswerRecordCount);

            Assert.AreEqual(0, header.AuthorityRecordCount);
            Assert.AreEqual(0, header.AdditionalRecordCount);
        }

        [Test]
        public void HeaderWithAuthorityRecordCount() {
            byte[] content = Helper.ReadFixture("Header", "nscount");
            Header header = Header.FromArray(content);

            Assert.AreEqual(0, header.Id);
            Assert.AreEqual(false, header.Response);
            Assert.AreEqual(OperationCode.Query, header.OperationCode);
            Assert.AreEqual(false, header.AuthorativeServer);
            Assert.AreEqual(false, header.Truncated);
            Assert.AreEqual(false, header.RecursionDesired);
            Assert.AreEqual(false, header.RecursionAvailable);
            Assert.AreEqual(ResponseCode.NoError, header.ResponseCode);
            Assert.AreEqual(0, header.QuestionCount);
            Assert.AreEqual(0, header.AnswerRecordCount);

            Assert.AreEqual(1, header.AuthorityRecordCount);

            Assert.AreEqual(0, header.AdditionalRecordCount);
        }

        [Test]
        public void HeaderWithAdditionalRecordCount() {
            byte[] content = Helper.ReadFixture("Header", "arcount");
            Header header = Header.FromArray(content);

            Assert.AreEqual(0, header.Id);
            Assert.AreEqual(false, header.Response);
            Assert.AreEqual(OperationCode.Query, header.OperationCode);
            Assert.AreEqual(false, header.AuthorativeServer);
            Assert.AreEqual(false, header.Truncated);
            Assert.AreEqual(false, header.RecursionDesired);
            Assert.AreEqual(false, header.RecursionAvailable);
            Assert.AreEqual(ResponseCode.NoError, header.ResponseCode);
            Assert.AreEqual(0, header.QuestionCount);
            Assert.AreEqual(0, header.AnswerRecordCount);
            Assert.AreEqual(0, header.AuthorityRecordCount);

            Assert.AreEqual(1, header.AdditionalRecordCount);
        }

        [Test]
        public void HeaderWithAllSet() {
            byte[] content = Helper.ReadFixture("Header", "all");
            Header header = Header.FromArray(content);

            Assert.AreEqual(1, header.Id);
            Assert.AreEqual(true, header.Response);
            Assert.AreEqual(OperationCode.Status, header.OperationCode);
            Assert.AreEqual(true, header.AuthorativeServer);
            Assert.AreEqual(true, header.Truncated);
            Assert.AreEqual(true, header.RecursionDesired);
            Assert.AreEqual(true, header.RecursionAvailable);
            Assert.AreEqual(ResponseCode.ServerFailure, header.ResponseCode);
            Assert.AreEqual(1, header.QuestionCount);
            Assert.AreEqual(1, header.AnswerRecordCount);
            Assert.AreEqual(1, header.AuthorityRecordCount);
            Assert.AreEqual(1, header.AdditionalRecordCount);
        }
    }
}
