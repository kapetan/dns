using Xunit;
using DNS.Protocol;

namespace DNS.Tests.Protocol {

    public class ParseHeaderTest {
        [Fact]
        public void EmptyHeader() {
            byte[] content = Helper.ReadFixture("Header", "empty");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.Equal(false, header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.Equal(false, header.AuthorativeServer);
            Assert.Equal(false, header.Truncated);
            Assert.Equal(false, header.RecursionDesired);
            Assert.Equal(false, header.RecursionAvailable);
            Assert.Equal(false, header.AuthenticData);
            Assert.Equal(false, header.CheckingDisabled);
            Assert.Equal(ResponseCode.NoError, header.ResponseCode);
            Assert.Equal(0, header.QuestionCount);
            Assert.Equal(0, header.AnswerRecordCount);
            Assert.Equal(0, header.AuthorityRecordCount);
            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithId() {
            byte[] content = Helper.ReadFixture("Header", "id");
            Header header = Header.FromArray(content);

            Assert.Equal(1, header.Id);

            Assert.Equal(false, header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.Equal(false, header.AuthorativeServer);
            Assert.Equal(false, header.Truncated);
            Assert.Equal(false, header.RecursionDesired);
            Assert.Equal(false, header.RecursionAvailable);
            Assert.Equal(false, header.AuthenticData);
            Assert.Equal(false, header.CheckingDisabled);
            Assert.Equal(ResponseCode.NoError, header.ResponseCode);
            Assert.Equal(0, header.QuestionCount);
            Assert.Equal(0, header.AnswerRecordCount);
            Assert.Equal(0, header.AuthorityRecordCount);
            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithQueryResponseFlag() {
            byte[] content = Helper.ReadFixture("Header", "qr");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);

            Assert.Equal(true, header.Response);

            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.Equal(false, header.AuthorativeServer);
            Assert.Equal(false, header.Truncated);
            Assert.Equal(false, header.RecursionDesired);
            Assert.Equal(false, header.RecursionAvailable);
            Assert.Equal(false, header.AuthenticData);
            Assert.Equal(false, header.CheckingDisabled);
            Assert.Equal(ResponseCode.NoError, header.ResponseCode);
            Assert.Equal(0, header.QuestionCount);
            Assert.Equal(0, header.AnswerRecordCount);
            Assert.Equal(0, header.AuthorityRecordCount);
            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithOperationCode() {
            byte[] content = Helper.ReadFixture("Header", "opcode");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.Equal(false, header.Response);

            Assert.Equal(OperationCode.Status, header.OperationCode);

            Assert.Equal(false, header.AuthorativeServer);
            Assert.Equal(false, header.Truncated);
            Assert.Equal(false, header.RecursionDesired);
            Assert.Equal(false, header.RecursionAvailable);
            Assert.Equal(false, header.AuthenticData);
            Assert.Equal(false, header.CheckingDisabled);
            Assert.Equal(ResponseCode.NoError, header.ResponseCode);
            Assert.Equal(0, header.QuestionCount);
            Assert.Equal(0, header.AnswerRecordCount);
            Assert.Equal(0, header.AuthorityRecordCount);
            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithAuthorativeAnswerFlag() {
            byte[] content = Helper.ReadFixture("Header", "aa");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.Equal(false, header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);

            Assert.Equal(true, header.AuthorativeServer);

            Assert.Equal(false, header.Truncated);
            Assert.Equal(false, header.RecursionDesired);
            Assert.Equal(false, header.RecursionAvailable);
            Assert.Equal(false, header.AuthenticData);
            Assert.Equal(false, header.CheckingDisabled);
            Assert.Equal(ResponseCode.NoError, header.ResponseCode);
            Assert.Equal(0, header.QuestionCount);
            Assert.Equal(0, header.AnswerRecordCount);
            Assert.Equal(0, header.AuthorityRecordCount);
            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithTruncatedFlag() {
            byte[] content = Helper.ReadFixture("Header", "tc");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.Equal(false, header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.Equal(false, header.AuthorativeServer);

            Assert.Equal(true, header.Truncated);

            Assert.Equal(false, header.RecursionDesired);
            Assert.Equal(false, header.RecursionAvailable);
            Assert.Equal(false, header.AuthenticData);
            Assert.Equal(false, header.CheckingDisabled);
            Assert.Equal(ResponseCode.NoError, header.ResponseCode);
            Assert.Equal(0, header.QuestionCount);
            Assert.Equal(0, header.AnswerRecordCount);
            Assert.Equal(0, header.AuthorityRecordCount);
            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithRecusionDesiredFlag() {
            byte[] content = Helper.ReadFixture("Header", "rd");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.Equal(false, header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.Equal(false, header.AuthorativeServer);
            Assert.Equal(false, header.Truncated);

            Assert.Equal(true, header.RecursionDesired);

            Assert.Equal(false, header.RecursionAvailable);
            Assert.Equal(false, header.AuthenticData);
            Assert.Equal(false, header.CheckingDisabled);
            Assert.Equal(ResponseCode.NoError, header.ResponseCode);
            Assert.Equal(0, header.QuestionCount);
            Assert.Equal(0, header.AnswerRecordCount);
            Assert.Equal(0, header.AuthorityRecordCount);
            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithRecusionAvailableFlag() {
            byte[] content = Helper.ReadFixture("Header", "ra");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.Equal(false, header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.Equal(false, header.AuthorativeServer);
            Assert.Equal(false, header.Truncated);
            Assert.Equal(false, header.RecursionDesired);

            Assert.Equal(true, header.RecursionAvailable);

            Assert.Equal(false, header.AuthenticData);
            Assert.Equal(false, header.CheckingDisabled);
            Assert.Equal(ResponseCode.NoError, header.ResponseCode);
            Assert.Equal(0, header.QuestionCount);
            Assert.Equal(0, header.AnswerRecordCount);
            Assert.Equal(0, header.AuthorityRecordCount);
            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithAuthenticDataFlag() {
            byte[] content = Helper.ReadFixture("Header", "ad");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.Equal(false, header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.Equal(false, header.AuthorativeServer);
            Assert.Equal(false, header.Truncated);
            Assert.Equal(false, header.RecursionDesired);
            Assert.Equal(false, header.RecursionAvailable);

            Assert.Equal(true, header.AuthenticData);

            Assert.Equal(false, header.CheckingDisabled);
            Assert.Equal(ResponseCode.NoError, header.ResponseCode);
            Assert.Equal(0, header.QuestionCount);
            Assert.Equal(0, header.AnswerRecordCount);
            Assert.Equal(0, header.AuthorityRecordCount);
            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithCheckingDisabledFlag() {
            byte[] content = Helper.ReadFixture("Header", "cd");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.Equal(false, header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.Equal(false, header.AuthorativeServer);
            Assert.Equal(false, header.Truncated);
            Assert.Equal(false, header.RecursionDesired);
            Assert.Equal(false, header.RecursionAvailable);
            Assert.Equal(false, header.AuthenticData);

            Assert.Equal(true, header.CheckingDisabled);

            Assert.Equal(ResponseCode.NoError, header.ResponseCode);
            Assert.Equal(0, header.QuestionCount);
            Assert.Equal(0, header.AnswerRecordCount);
            Assert.Equal(0, header.AuthorityRecordCount);
            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithResponseCode() {
            byte[] content = Helper.ReadFixture("Header", "rcode");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.Equal(false, header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.Equal(false, header.AuthorativeServer);
            Assert.Equal(false, header.Truncated);
            Assert.Equal(false, header.RecursionDesired);
            Assert.Equal(false, header.RecursionAvailable);
            Assert.Equal(false, header.AuthenticData);
            Assert.Equal(false, header.CheckingDisabled);

            Assert.Equal(ResponseCode.ServerFailure, header.ResponseCode);

            Assert.Equal(0, header.QuestionCount);
            Assert.Equal(0, header.AnswerRecordCount);
            Assert.Equal(0, header.AuthorityRecordCount);
            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithQuestionCount() {
            byte[] content = Helper.ReadFixture("Header", "qdcount");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.Equal(false, header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.Equal(false, header.AuthorativeServer);
            Assert.Equal(false, header.Truncated);
            Assert.Equal(false, header.RecursionDesired);
            Assert.Equal(false, header.RecursionAvailable);
            Assert.Equal(false, header.AuthenticData);
            Assert.Equal(false, header.CheckingDisabled);
            Assert.Equal(ResponseCode.NoError, header.ResponseCode);

            Assert.Equal(1, header.QuestionCount);

            Assert.Equal(0, header.AnswerRecordCount);
            Assert.Equal(0, header.AuthorityRecordCount);
            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithAnswerRecordCount() {
            byte[] content = Helper.ReadFixture("Header", "ancount");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.Equal(false, header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.Equal(false, header.AuthorativeServer);
            Assert.Equal(false, header.Truncated);
            Assert.Equal(false, header.RecursionDesired);
            Assert.Equal(false, header.RecursionAvailable);
            Assert.Equal(false, header.AuthenticData);
            Assert.Equal(false, header.CheckingDisabled);
            Assert.Equal(ResponseCode.NoError, header.ResponseCode);
            Assert.Equal(0, header.QuestionCount);

            Assert.Equal(1, header.AnswerRecordCount);

            Assert.Equal(0, header.AuthorityRecordCount);
            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithAuthorityRecordCount() {
            byte[] content = Helper.ReadFixture("Header", "nscount");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.Equal(false, header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.Equal(false, header.AuthorativeServer);
            Assert.Equal(false, header.Truncated);
            Assert.Equal(false, header.RecursionDesired);
            Assert.Equal(false, header.RecursionAvailable);
            Assert.Equal(false, header.AuthenticData);
            Assert.Equal(false, header.CheckingDisabled);
            Assert.Equal(ResponseCode.NoError, header.ResponseCode);
            Assert.Equal(0, header.QuestionCount);
            Assert.Equal(0, header.AnswerRecordCount);

            Assert.Equal(1, header.AuthorityRecordCount);

            Assert.Equal(0, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithAdditionalRecordCount() {
            byte[] content = Helper.ReadFixture("Header", "arcount");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.Equal(false, header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.Equal(false, header.AuthorativeServer);
            Assert.Equal(false, header.Truncated);
            Assert.Equal(false, header.RecursionDesired);
            Assert.Equal(false, header.RecursionAvailable);
            Assert.Equal(false, header.AuthenticData);
            Assert.Equal(false, header.CheckingDisabled);
            Assert.Equal(ResponseCode.NoError, header.ResponseCode);
            Assert.Equal(0, header.QuestionCount);
            Assert.Equal(0, header.AnswerRecordCount);
            Assert.Equal(0, header.AuthorityRecordCount);

            Assert.Equal(1, header.AdditionalRecordCount);
        }

        [Fact]
        public void HeaderWithAllSet() {
            byte[] content = Helper.ReadFixture("Header", "all");
            Header header = Header.FromArray(content);

            Assert.Equal(1, header.Id);
            Assert.Equal(true, header.Response);
            Assert.Equal(OperationCode.Status, header.OperationCode);
            Assert.Equal(true, header.AuthorativeServer);
            Assert.Equal(true, header.Truncated);
            Assert.Equal(true, header.RecursionDesired);
            Assert.Equal(true, header.RecursionAvailable);
            Assert.Equal(true, header.AuthenticData);
            Assert.Equal(true, header.CheckingDisabled);
            Assert.Equal(ResponseCode.ServerFailure, header.ResponseCode);
            Assert.Equal(1, header.QuestionCount);
            Assert.Equal(1, header.AnswerRecordCount);
            Assert.Equal(1, header.AuthorityRecordCount);
            Assert.Equal(1, header.AdditionalRecordCount);
        }
    }
}
