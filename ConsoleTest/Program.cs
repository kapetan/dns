using DNS.Client;
using DNS.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();
            Console.ReadKey();
        }

        static async void Run()
        {
            try
            {
                // Bind to a Domain Name Server
                DnsClient client = new DnsClient("8.8.8.8");

                // Create request bound to 8.8.8.8
                ClientRequest request = client.Create();

                // Returns a list of IPs
                IList<IPAddress> ips = await client.Lookup("framesoft.ir");

                // Get the domain name belonging to the IP (google.com)
                //string domain = await client.Reverse("173.194.69.100");
            }
            catch (Exception ex)
            {

            }
        }
    }
}
