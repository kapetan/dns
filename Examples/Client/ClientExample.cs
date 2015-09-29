using System;
using System.Collections.Generic;
using System.Net;
using DNS.Client;

namespace Examples.Client
{
    class ClientExample {
        public static void Main(string[] args) {
            DnsClient client = new DnsClient("8.8.8.8");

            foreach (string domain in args) {
                IList<IPAddress> ips = client.Lookup(domain);
                Console.WriteLine("{0} => {1}", domain, string.Join(", ", ips));
            }
        }
    }
}
