using System;
using Xunit;
using DNS.Protocol;

namespace DNS.Tests.Protocol {
    
    public class SerializeHeaderTest {
        [Fact]
        public void EmptyHeader() {
            Header header = new Header();
            byte[] content = Helper.ReadFixture("Header", "empty");

            Assert.Equal(content, header.ToArray());
        }

        [Fact]
        public void HeaderWithId() {
            Header header = new Header();
            header.Id = 1;
            byte[] content = Helper.ReadFixture("Header", "id");

            Assert.Equal(content, header.ToArray());
        }

        [Fact]
        public void HeaderWithQueryResponseFlag() {
            Header header = new Header();
            header.Response = true;
            byte[] content = Helper.ReadFixture("Header", "qr");

            Assert.Equal(content, header.ToArray());
        }

        [Fact]
        public void HeaderWithOperationCode() {
            Header header = new Header();
            header.OperationCode = OperationCode.Status;
            byte[] content = Helper.ReadFixture("Header", "opcode");

            Assert.Equal(content, header.ToArray());
        }

        [Fact]
        public void HeaderWithAuthorativeAnswerFlag() {
            Header header = new Header();
            header.AuthorativeServer = true;
            byte[] content = Helper.ReadFixture("Header", "aa");

            Assert.Equal(content, header.ToArray());
        }

        [Fact]
        public void HeaderWithTruncatedFlag() {
            Header header = new Header();
            header.Truncated = true;
            byte[] content = Helper.ReadFixture("Header", "tc");

            Assert.Equal(content, header.ToArray());
        }

        [Fact]
        public void HeaderWithRecusionDesiredFlag() {
            Header header = new Header();
            header.RecursionDesired = true;
            byte[] content = Helper.ReadFixture("Header", "rd");

            Assert.Equal(content, header.ToArray());
        }

        [Fact]
        public void HeaderWithRecusionAvailableFlag() {
            Header header = new Header();
            header.RecursionAvailable = true;
            byte[] content = Helper.ReadFixture("Header", "ra");

            Assert.Equal(content, header.ToArray());
        }

        [Fact]
        public void HeaderWithResponseCode() {
            Header header = new Header();
            header.ResponseCode = ResponseCode.ServerFailure;
            byte[] content = Helper.ReadFixture("Header", "rcode");

            Assert.Equal(content, header.ToArray());
        }

        [Fact]
        public void HeaderWithQuestionCount() {
            Header header = new Header();
            header.QuestionCount = 1;
            byte[] content = Helper.ReadFixture("Header", "qdcount");

            Assert.Equal(content, header.ToArray());
        }

        [Fact]
        public void HeaderWithAnswerRecordCount() {
            Header header = new Header();
            header.AnswerRecordCount = 1;
            byte[] content = Helper.ReadFixture("Header", "ancount");

            Assert.Equal(content, header.ToArray());
        }

        [Fact]
        public void HeaderWithAuthorityRecordCount() {
            Header header = new Header();
            header.AuthorityRecordCount = 1;
            byte[] content = Helper.ReadFixture("Header", "nscount");

            Assert.Equal(content, header.ToArray());
        }

        [Fact]
        public void HeaderWithAdditionalRecordCount() {
            Header header = new Header();
            header.AdditionalRecordCount = 1;
            byte[] content = Helper.ReadFixture("Header", "arcount");

            Assert.Equal(content, header.ToArray());
        }

        [Fact]
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

            Assert.Equal(content, header.ToArray());
        }
    }
}
