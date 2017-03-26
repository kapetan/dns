using System;
using DNS.Server;

namespace Examples.Server {
    class ServerExample {
        public static void Main(string[] args) {
            DnsServer server = new DnsServer("8.8.8.8");

            foreach(string domain in args) {
                Console.WriteLine("Redirecting {0} to localhost", domain);
                server.MasterFile.AddIPAddressResourceRecord(domain, "127.0.0.1");
            }

            server.Responded += (request, response) => Console.WriteLine("{0} => {1}", request, response);
            server.Listen().GetAwaiter().GetResult();
        }
    }
}
