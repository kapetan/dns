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
		[Fact]
		public async Task TestResolve() {
			MasterFile masterFile = new MasterFile();
			masterFile.AddIPAddressResourceRecord("google.com", "192.168.0.1");
			{
				//Test matching case.
				IRequest clientRequest = new Request();
				Question clientRequestQuestion = new Question(new Domain("google.com"), RecordType.A);

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

				Assert.Equal(RecordType.A, clientResponseQuestion.Type);
				Assert.Equal("google.com", clientResponseQuestion.Name.ToString());

				var clientResponseAnswer = clientResponse.AnswerRecords[0];
				Assert.Equal(RecordType.A, clientResponseAnswer.Type);
				Assert.Equal("google.com", clientResponseAnswer.Name.ToString());
				Assert.Equal("192.168.0.1", ((IPAddressResourceRecord)clientResponseAnswer).IPAddress.ToString());
			}
			{
				//Test different casing.
				IRequest clientRequest = new Request();
				Question clientRequestQuestion = new Question(new Domain("gooGLE.com"), RecordType.A);

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

				Assert.Equal(RecordType.A, clientResponseQuestion.Type);
				Assert.Equal("gooGLE.com", clientResponseQuestion.Name.ToString());

				var clientResponseAnswer = clientResponse.AnswerRecords[0];
				Assert.Equal(RecordType.A, clientResponseAnswer.Type);
				Assert.Equal("google.com", clientResponseAnswer.Name.ToString());
				Assert.Equal("192.168.0.1", ((IPAddressResourceRecord)clientResponseAnswer).IPAddress.ToString());
			}
			{
				//Test RecordType.ANY
				IRequest clientRequest = new Request();
				Question clientRequestQuestion = new Question(new Domain("google.com"), RecordType.ANY);

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

				Assert.Equal(RecordType.ANY, clientResponseQuestion.Type);
				Assert.Equal("google.com", clientResponseQuestion.Name.ToString());

				var clientResponseAnswer = clientResponse.AnswerRecords[0];
				Assert.Equal(RecordType.A, clientResponseAnswer.Type);
				Assert.Equal("google.com", clientResponseAnswer.Name.ToString());
				Assert.Equal("192.168.0.1", ((IPAddressResourceRecord) clientResponseAnswer).IPAddress.ToString());
			}
		}
	}
}
