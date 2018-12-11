using System;
using System.Threading.Tasks;
using DNS.Server;
using DNS.Client;
using DNS.Protocol.ResourceRecords;
using System.Linq;

namespace Examples.Server
{
    class ServerExample
    {
        public static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        public async static Task MainAsync(string[] args)
        {
            MasterFile masterFile = new MasterFile();
            DnsServer server = new DnsServer(masterFile, "192.168.0.30", 53);

            //masterFile.AddIPAddressResourceRecord("*.whereami.glauxsoft.com", "212.243.20.2");
            masterFile.AddIPAddressResourceRecord("*.redirect.glauxsoft.com", "212.243.20.2");

            server.Responded += (sender, e) =>
            {
                if (e.Response.AnswerRecords.FirstOrDefault()?.Name.ToString().EndsWith(".redirect.glauxsoft.com", StringComparison.CurrentCultureIgnoreCase) == true)
                {
                    var res = e.Response.AnswerRecords.First() as IPAddressResourceRecord;
                    var newRec = new IPAddressResourceRecord(e.Request.Questions.First().Name, res.IPAddress, TimeSpan.FromSeconds(60));
                    e.Response.AnswerRecords.Clear();
                    e.Response.AnswerRecords.Add(newRec);

                    Console.WriteLine(e.Response.AnswerRecords.FirstOrDefault()?.Name.ToString());
                }

                //Console.WriteLine("{0} => {1}", e.Request, e.Response);
            };

            server.Listening += (sender, e) => Console.WriteLine("Listening");
            server.Errored += (sender, e) =>
            {
                Console.WriteLine("Errored: {0}", e.Exception);
                ResponseException responseError = e.Exception as ResponseException;
                if (responseError != null) Console.WriteLine(responseError.Response);
            };

            await server.Listen();
        }
    }


    internal class MyResolver : MasterFile
    {



    }
}
