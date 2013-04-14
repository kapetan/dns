using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using DNS.Protocol;

namespace DNS {
    public class Server {
        public static void Main(string[] args) {
            Server server = null;

            (new Thread(() => {
                server = new Server();

                server.Responded += (request, response) => Console.WriteLine("{0} => {1}", request, response);

                Console.WriteLine("Starting");

                server.Run();
            })).Start();

            Thread.Sleep(2000);

            Client client = new Client("127.0.0.1", 8000);

            Console.WriteLine(DNS.Marshalling.Object.Dump(client.Lookup("google.com")));
            Console.WriteLine(DNS.Marshalling.Object.Dump(client.Lookup("dr.dk")));
        }

        private const int PORT = 8000;
        private const int UDP_TIMEOUT = 2000;

        public delegate void RequestedEventHandler(IRequest request);
        public delegate void RespondedEventHandler(IRequest request, IResponse response);

        private UdpClient udp;
        private EventEmitter emitter;
        private Client client;

        public event RequestedEventHandler Requested;
        public event RespondedEventHandler Responded;

        public void Run() {
            emitter = new EventEmitter();
            udp = new UdpClient(PORT);
            client = new Client("8.8.8.8");

            IPEndPoint local = new IPEndPoint(IPAddress.Any, PORT);

            emitter.Run();
            udp.Client.SendTimeout = UDP_TIMEOUT;

            while (true) {
                byte[] clientMessage = udp.Receive(ref local);

                Thread task = new Thread(() => {
                    try {
                        ClientRequest request = client.FromArray(clientMessage);
                        emitter.Schedule(() => OnRequested(request));

                        ClientResponse response = request.Resolve();
                        emitter.Schedule(() => Responded(request, response));

                        udp.Send(response.OriginalMessage, response.OriginalMessage.Length, local);
                    } 
                    catch(SocketException) {}
                    catch(ResponseException) {}
                });

                task.Start();
            }
        }

        public void Close() {
            if (udp != null) {
                emitter.Stop();
                udp.Close();
            }
        }

        protected virtual void OnRequested(ClientRequest request) {
            RequestedEventHandler handlers = Requested;
            if (handlers != null) handlers(request);
        }

        protected virtual void OnResponded(ClientRequest request, Response response) {
            RespondedEventHandler handlers = Responded;
            if (handlers != null) handlers(request, response);
        }
    }

    internal class EventEmitter {
        public delegate void Emit();

        private CancellationTokenSource tokenSource;
        private BlockingCollection<Emit> queue;

        public void Schedule(Emit emit) {
            if (queue != null) {
                queue.Add(emit);
            }
        }

        public void Run() {
            tokenSource = new CancellationTokenSource();
            queue = new BlockingCollection<Emit>();

            (new Thread(() => {
                try {
                    while (true) {
                        Emit emit = queue.Take(tokenSource.Token);
                        emit();
                    }
                } catch (OperationCanceledException) { }
            })).Start();
        }

        public void Stop() {
            if (tokenSource != null) {
                tokenSource.Cancel();
            }
        }
    }
}
