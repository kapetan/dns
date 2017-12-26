using System;
using System.Threading.Tasks;
using DNS.Client;
using DNS.Server;

namespace Examples.ClientServer {
    class ClientServerExample {
        private const int PORT = 53535;

        public static void Main(string[] args) {
            MainAsync().Wait();
        }

        public async static Task MainAsync() {
            DnsServer server = new DnsServer("8.8.8.8");

            server.Requested += (request) => Console.WriteLine("Requested: {0}", request);
            server.Responded += (request, response) => Console.WriteLine("Responded: {0} => {1}", request, response);
            server.Errored += (e) => Console.WriteLine("Errored: {0}", e.Message);
            server.Listening += () => Console.WriteLine("Listening");

            server.MasterFile.AddIPAddressResourceRecord("google.com", "127.0.0.1");

            server.Listening += async () => {
                DnsClient client = new DnsClient("127.0.0.1", PORT);

                await client.Lookup("google.com");
                await client.Lookup("cnn.com");

                server.Dispose();
            };

            await server.Listen(PORT);
        }
    }
}
