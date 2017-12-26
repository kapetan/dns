using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using DNS.Client;
using DNS.Server;

namespace DNS.Tests.ClientServer {

    public class ClientTest {
        private const int PORT = 64646;

        [Fact]
        public async Task ClientLookup() {
            await CreateServer(async (server) => {
                server.MasterFile.AddIPAddressResourceRecord("google.com", "192.168.0.1");

                DnsClient client = new DnsClient("127.0.0.1", PORT);
                IList<IPAddress> ips = await client.Lookup("google.com");

                Assert.Equal(1, ips.Count);
                Assert.Equal("192.168.0.1", ips[0].ToString());
            });
        }

        [Fact]
        public async Task ClientReverse() {
            await CreateServer(async (server) => {
                server.MasterFile.AddPointerResourceRecord("192.168.0.1", "google.com");

                DnsClient client = new DnsClient("127.0.0.1", PORT);
                string domain = await client.Reverse("192.168.0.1");

                Assert.Equal("google.com", domain);
            });
        }

        private async Task CreateServer(Func<DnsServer, Task> handler) {
            DnsServer server = new DnsServer("8.8.8.8");
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            server.Errored += (e) => {
                tcs.TrySetException(e);
            };

            server.Listening += async () => {
                try {
                    await handler(server);
                    tcs.TrySetResult(null);
                } catch (Exception e) {
                    tcs.TrySetException(e);
                } finally {
                   server.Dispose();
                }
            };

            await Task.WhenAll(server.Listen(PORT), tcs.Task);
        }
    }
}
