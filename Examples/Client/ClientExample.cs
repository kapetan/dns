using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DNS.Client;
using DNS.Protocol;

namespace Examples.Client
{
    class ClientExample {
        public static void Main(string[] args) {
            MainAsync(args).Wait();
        }

        public async static Task MainAsync(string[] args) {
            DnsClient client = new DnsClient("8.8.8.8");

            foreach (string domain in args) {
                IList<IPAddress> ips = await client.Lookup(domain);
                Console.WriteLine("{0} => {1}", domain, string.Join(", ", ips));
            }
        }
    }
}
