using Xunit;
using DNS.Protocol;

namespace DNS.Tests.Protocol {

    public class ParseHeaderTest {
        [Fact]
        public void EmptyHeader() {
            byte[] content = Helper.ReadFixture("Header", "empty");
            Header header = Header.FromArray(content);

            Assert.Equal(0, header.Id);
            Assert.False(header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.False(header.AuthorativeServer);
            Assert.False(header.Truncated);
            Assert.False(header.RecursionDesired);
            Assert.False(header.RecursionAvailable);
            Assert.False(header.AuthenticData);
            Assert.False(header.CheckingDisabled);
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

            Assert.False(header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.False(header.AuthorativeServer);
            Assert.False(header.Truncated);
            Assert.False(header.RecursionDesired);
            Assert.False(header.RecursionAvailable);
            Assert.False(header.AuthenticData);
            Assert.False(header.CheckingDisabled);
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

            Assert.True(header.Response);

            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.False(header.AuthorativeServer);
            Assert.False(header.Truncated);
            Assert.False(header.RecursionDesired);
            Assert.False(header.RecursionAvailable);
            Assert.False(header.AuthenticData);
            Assert.False(header.CheckingDisabled);
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
            Assert.False(header.Response);

            Assert.Equal(OperationCode.Status, header.OperationCode);

            Assert.False(header.AuthorativeServer);
            Assert.False(header.Truncated);
            Assert.False(header.RecursionDesired);
            Assert.False(header.RecursionAvailable);
            Assert.False(header.AuthenticData);
            Assert.False(header.CheckingDisabled);
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
            Assert.False(header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);

            Assert.True(header.AuthorativeServer);

            Assert.False(header.Truncated);
            Assert.False(header.RecursionDesired);
            Assert.False(header.RecursionAvailable);
            Assert.False(header.AuthenticData);
            Assert.False(header.CheckingDisabled);
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
            Assert.False(header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.False(header.AuthorativeServer);

            Assert.True(header.Truncated);

            Assert.False(header.RecursionDesired);
            Assert.False(header.RecursionAvailable);
            Assert.False(header.AuthenticData);
            Assert.False(header.CheckingDisabled);
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
            Assert.False(header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.False(header.AuthorativeServer);
            Assert.False(header.Truncated);

            Assert.True(header.RecursionDesired);

            Assert.False(header.RecursionAvailable);
            Assert.False(header.AuthenticData);
            Assert.False(header.CheckingDisabled);
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
            Assert.False(header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.False(header.AuthorativeServer);
            Assert.False(header.Truncated);
            Assert.False(header.RecursionDesired);

            Assert.True(header.RecursionAvailable);

            Assert.False(header.AuthenticData);
            Assert.False(header.CheckingDisabled);
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
            Assert.False(header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.False(header.AuthorativeServer);
            Assert.False(header.Truncated);
            Assert.False(header.RecursionDesired);
            Assert.False(header.RecursionAvailable);

            Assert.True(header.AuthenticData);

            Assert.False(header.CheckingDisabled);
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
            Assert.False(header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.False(header.AuthorativeServer);
            Assert.False(header.Truncated);
            Assert.False(header.RecursionDesired);
            Assert.False(header.RecursionAvailable);
            Assert.False(header.AuthenticData);

            Assert.True(header.CheckingDisabled);

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
            Assert.False(header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.False(header.AuthorativeServer);
            Assert.False(header.Truncated);
            Assert.False(header.RecursionDesired);
            Assert.False(header.RecursionAvailable);
            Assert.False(header.AuthenticData);
            Assert.False(header.CheckingDisabled);

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
            Assert.False(header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.False(header.AuthorativeServer);
            Assert.False(header.Truncated);
            Assert.False(header.RecursionDesired);
            Assert.False(header.RecursionAvailable);
            Assert.False(header.AuthenticData);
            Assert.False(header.CheckingDisabled);
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
            Assert.False(header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.False(header.AuthorativeServer);
            Assert.False(header.Truncated);
            Assert.False(header.RecursionDesired);
            Assert.False(header.RecursionAvailable);
            Assert.False(header.AuthenticData);
            Assert.False(header.CheckingDisabled);
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
            Assert.False(header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.False(header.AuthorativeServer);
            Assert.False(header.Truncated);
            Assert.False(header.RecursionDesired);
            Assert.False(header.RecursionAvailable);
            Assert.False(header.AuthenticData);
            Assert.False(header.CheckingDisabled);
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
            Assert.False(header.Response);
            Assert.Equal(OperationCode.Query, header.OperationCode);
            Assert.False(header.AuthorativeServer);
            Assert.False(header.Truncated);
            Assert.False(header.RecursionDesired);
            Assert.False(header.RecursionAvailable);
            Assert.False(header.AuthenticData);
            Assert.False(header.CheckingDisabled);
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
            Assert.True(header.Response);
            Assert.Equal(OperationCode.Status, header.OperationCode);
            Assert.True(header.AuthorativeServer);
            Assert.True(header.Truncated);
            Assert.True(header.RecursionDesired);
            Assert.True(header.RecursionAvailable);
            Assert.True(header.AuthenticData);
            Assert.True(header.CheckingDisabled);
            Assert.Equal(ResponseCode.ServerFailure, header.ResponseCode);
            Assert.Equal(1, header.QuestionCount);
            Assert.Equal(1, header.AnswerRecordCount);
            Assert.Equal(1, header.AuthorityRecordCount);
            Assert.Equal(1, header.AdditionalRecordCount);
        }
    }
}
