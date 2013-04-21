using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using DNS.Protocol;

namespace DNS {
    class Example {
        public static void Main(string[] args) {
            Server server = null;

            (new Thread(() => {
                server = new Server("8.8.8.8");

                server.Responded += (request, response) => Console.WriteLine("{0} => {1}", request, response);

                server.MasterFile.AddIPAddressResourceRecord("google.com", "127.0.0.1");

                Console.WriteLine("Starting");

                server.Listen();
            })).Start();

            Thread.Sleep(2000);

            Client client = new Client("127.0.0.1");

            //Console.WriteLine(DNS.Marshalling.Object.Dump(client.Reverse(IPAddress.Parse("173.194.69.100"))));
            Console.WriteLine(DNS.Marshalling.Object.Dump(client.Lookup("google.com")));
            //Console.WriteLine(DNS.Marshalling.Object.Dump(client.Lookup("dr.dk")));

            server.Close();
        }
    }
}
