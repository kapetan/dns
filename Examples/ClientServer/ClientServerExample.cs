using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using DNS.Client;
using DNS.Server;

namespace Examples.ClientServer {
    class ClientServerExample {
        public static void Main(string[] args) {
            DnsServer server = null;

            (new Thread(() => {
                server = new DnsServer("8.8.8.8");

                server.Requested += (request) => Console.WriteLine("Requested: {0}", request);
                server.Responded += (request, response) => Console.WriteLine("Responded: {0} => {1}", request, response);

                server.MasterFile.AddIPAddressResourceRecord("google.com", "127.0.0.1");

                server.Listen().GetAwaiter().GetResult();
            })).Start();

            Thread.Sleep(1000);

            DnsClient client = new DnsClient("127.0.0.1");

            client.Lookup("google.com").GetAwaiter().GetResult();
            client.Lookup("cnn.com").GetAwaiter().GetResult();

            server.Dispose();
        }
    }
}
