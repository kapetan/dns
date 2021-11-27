﻿using System;
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
            MasterFile masterFile = new MasterFile();
            DnsServer server = new DnsServer(masterFile, "8.8.8.8");

            masterFile.AddIPAddressResourceRecord("google.com", "127.0.0.1");

            server.Requested += (sender, e) => Console.WriteLine("Requested: {0}", e.Request);
            server.Responded += (sender, e) => Console.WriteLine("Responded: {0} => {1}", e.Request, e.Response);
            server.Errored += (sender, e) => Console.WriteLine("Errored: {0}", e.Exception.Message);
            server.Listening += (sender, e) => Console.WriteLine("Listening");

            server.Listening += async (sender, e) => {
                DnsClient client = new DnsClient("127.0.0.1", PORT);

                await client.Lookup("google.com").ConfigureAwait(false);
                await client.Lookup("cnn.com").ConfigureAwait(false);

                server.Dispose();
            };

            await server.Listen(PORT).ConfigureAwait(false);
        }
    }
}
