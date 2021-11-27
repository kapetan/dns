using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Server;
using Xunit;

namespace DNS.Tests.Server {

    public class MasterFileTest {
        [Theory]
        [InlineData(RecordType.A, "google.com")]
        [InlineData(RecordType.A, "gooGLE.com")]
        [InlineData(RecordType.ANY, "google.com")]
        [InlineData(RecordType.ANY, "gooGLE.com")]
        public async Task ResolveRecord(RecordType recordType, string domain) {
            MasterFile masterFile = new MasterFile();
            masterFile.AddIPAddressResourceRecord("google.com", "192.168.0.1");

            IRequest clientRequest = new Request();
            Question clientRequestQuestion = new Question(new Domain(domain), recordType);

            clientRequest.Id = 1;
            clientRequest.Questions.Add(clientRequestQuestion);
            clientRequest.OperationCode = OperationCode.Query;

            IResponse clientResponse = await masterFile.Resolve(clientRequest);

            Assert.Equal(1, clientResponse.Id);
            Assert.Equal(1, clientResponse.Questions.Count);
            Assert.Equal(1, clientResponse.AnswerRecords.Count);
            Assert.Equal(0, clientResponse.AuthorityRecords.Count);
            Assert.Equal(0, clientResponse.AdditionalRecords.Count);

            Question clientResponseQuestion = clientResponse.Questions[0];

            Assert.Equal(recordType, clientResponseQuestion.Type);
            Assert.Equal(domain, clientResponseQuestion.Name.ToString());

            IResourceRecord clientResponseAnswer = clientResponse.AnswerRecords[0];
            Assert.Equal(RecordType.A, clientResponseAnswer.Type);
            Assert.Equal("google.com", clientResponseAnswer.Name.ToString());
            Assert.Equal("192.168.0.1", ((IPAddressResourceRecord) clientResponseAnswer).IPAddress.ToString());
        }
    }
}
