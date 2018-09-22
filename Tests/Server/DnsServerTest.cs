using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Xunit;
using DNS.Server;
using DNS.Client;
using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

namespace DNS.Tests.Server {

    public class DnsServerTest {
        private const int PORT = 64646;

        [Fact]
        public async Task ServerLookup() {
            await Create(new IPAddressRequestResolver(), async server => {
                DnsServer.RequestedEventArgs requestedEvent = null;
                DnsServer.RespondedEventArgs respondedEvent = null;
                DnsServer.ErroredEventArgs erroredEvent = null;

                server.Requested += (sender, e) => {
                    requestedEvent = e;
                };

                server.Responded += (sender, e) => {
                    respondedEvent = e;
                };

                server.Errored += (sender, e) => {
                    erroredEvent = e;
                };

                IRequest clientRequest = new Request();
                Question clientRequestQuestion = new Question(new Domain("google.com"), RecordType.A);

                clientRequest.Id = 1;
                clientRequest.Questions.Add(clientRequestQuestion);
                clientRequest.OperationCode = OperationCode.Query;

                IResponse clientResponse = await Resolve(clientRequest);

                Assert.Equal(1, clientResponse.Id);
                Assert.Equal(1, clientResponse.Questions.Count);
                Assert.Equal(1, clientResponse.AnswerRecords.Count);
                Assert.Equal(0, clientResponse.AuthorityRecords.Count);
                Assert.Equal(0, clientResponse.AdditionalRecords.Count);

                Question clientResponseQuestion = clientResponse.Questions[0];

                Assert.Equal(RecordType.A, clientResponseQuestion.Type);
                Assert.Equal("google.com", clientResponseQuestion.Name.ToString());

                IResourceRecord clientResponseRecord = clientResponse.AnswerRecords[0];

                Assert.Equal("google.com", clientResponseRecord.Name.ToString());
                Assert.Equal(Helper.GetArray<byte>(192, 168, 0, 1), clientResponseRecord.Data);
                Assert.Equal(RecordType.A, clientResponseRecord.Type);

                Assert.NotNull(requestedEvent);

                Assert.NotNull(requestedEvent.Request);
                Assert.Equal(1, requestedEvent.Request.Id);
                Assert.Equal(1, requestedEvent.Request.Questions.Count);

                Question requestedRequestQuestion = requestedEvent.Request.Questions[0];

                Assert.Equal(RecordType.A, requestedRequestQuestion.Type);
                Assert.Equal("google.com", requestedRequestQuestion.Name.ToString());

                Assert.NotNull(respondedEvent);
                Assert.Equal(requestedEvent.Request, respondedEvent.Request);

                Assert.NotNull(respondedEvent.Response);

                Assert.Equal(1, respondedEvent.Response.Id);
                Assert.Equal(1, respondedEvent.Response.Questions.Count);
                Assert.Equal(1, respondedEvent.Response.AnswerRecords.Count);
                Assert.Equal(0, respondedEvent.Response.AuthorityRecords.Count);
                Assert.Equal(0, respondedEvent.Response.AdditionalRecords.Count);

                Question respondedResponseQuestion = respondedEvent.Response.Questions[0];

                Assert.Equal(RecordType.A, respondedResponseQuestion.Type);
                Assert.Equal("google.com", respondedResponseQuestion.Name.ToString());

                IResourceRecord respondedResponseRecord = respondedEvent.Response.AnswerRecords[0];

                Assert.Equal("google.com", respondedResponseRecord.Name.ToString());
                Assert.Equal(Helper.GetArray<byte>(192, 168, 0, 1), respondedResponseRecord.Data);
                Assert.Equal(RecordType.A, respondedResponseRecord.Type);

                Assert.Null(erroredEvent);
            });
        }

        private async static Task Create(IRequestResolver requestResolver, Func<DnsServer, Task> action) {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            DnsServer server = new DnsServer(requestResolver);

            server.Listening += async (sender, _) => {
                try {
                    await action(server);
                    tcs.SetResult(null);
                } catch(Exception e) {
                    tcs.SetException(e);
                } finally {
                    server.Dispose();
                }
            };

            await Task.WhenAll(server.Listen(PORT), tcs.Task);
        }

        private async static Task<IResponse> Resolve(IRequest request) {
            using (UdpClient udp = new UdpClient()) {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT);

                await udp.SendAsync(request.ToArray(), request.Size, endPoint);
                UdpReceiveResult result = await udp.ReceiveAsync();
                return Response.FromArray(result.Buffer);
            }
        }

        private class IPAddressRequestResolver : IRequestResolver {
            public Task<IResponse> Resolve(IRequest request) {
                IResponse response = Response.FromRequest(request);
                IResourceRecord record = new IPAddressResourceRecord(
                    new Domain("google.com"),
                    IPAddress.Parse("192.168.0.1"));

                response.AnswerRecords.Add(record);

                return Task.FromResult(response);
            }
        }
    }
}
