using System;
using System.Threading.Tasks;
using DNS.Server;
using DNS.Client;

namespace Examples.Server {
    class ServerExample {
        public static void Main(string[] args) {
            MainAsync(args).Wait();
        }

        public async static Task MainAsync(string[] args) {
            MasterFile masterFile = new MasterFile();
            DnsServer server = new DnsServer(masterFile, "8.8.8.8");

            foreach(string domain in args) {
                Console.WriteLine("Redirecting {0} to localhost", domain);
                masterFile.AddIPAddressResourceRecord(domain, "127.0.0.1");
            }

            server.Responded += (request, response) => Console.WriteLine("{0} => {1}", request, response);
            server.Listening += () => Console.WriteLine("Listening");
            server.Errored += (e) => {
                Console.WriteLine("Errored: {0}", e);
                ResponseException responseError = e as ResponseException;
                if(responseError != null) Console.WriteLine(responseError.Response);
            };

            await server.Listen();
        }
    }
}
