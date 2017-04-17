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

            server.MasterFile.AddIPAddressResourceRecord("google.com", "127.0.0.1");

            #pragma warning disable 4014
            server.Listen(PORT);
            #pragma warning restore 4014

            await Task.Delay(1000);

            DnsClient client = new DnsClient("127.0.0.1", PORT);

            await client.Lookup("google.com");
            await client.Lookup("cnn.com");

            server.Dispose();
        }
    }
}
