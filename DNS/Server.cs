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
        private const int DEFAULT_PORT = 53;
        private const int UDP_TIMEOUT = 2000;

        public delegate void RequestedEventHandler(IRequest request);
        public delegate void RespondedEventHandler(IRequest request, IResponse response);

        private IPEndPoint endServer;
        private MasterFile masterFile;

        private UdpClient udp;
        private EventEmitter emitter;
        private Client client;

        public event RequestedEventHandler Requested;
        public event RespondedEventHandler Responded;

        public Server(IPEndPoint endServer) {
            this.endServer = endServer;
            this.masterFile = new MasterFile();
        }

        public Server(IPAddress endServer, int port = DEFAULT_PORT) : this(new IPEndPoint(endServer, port)) {}
        public Server(string endServerIp, int port = DEFAULT_PORT) : this(IPAddress.Parse(endServerIp), port) {}

        public void Listen(int port = DEFAULT_PORT) {
            emitter = new EventEmitter();
            udp = new UdpClient(port);
            client = new Client(endServer);

            IPEndPoint local = new IPEndPoint(IPAddress.Any, port);

            emitter.Run();
            udp.Client.SendTimeout = UDP_TIMEOUT;

            while (true) {
                byte[] clientMessage = udp.Receive(ref local);

                Thread task = new Thread(() => {
                    try {
                        ClientRequest request = client.FromArray(clientMessage);
                        emitter.Schedule(() => OnRequested(request));

                        ClientResponse response = Resolve(request);

                        if (request.Questions.Count > 0) {
                            ClientResponse remote = request.Resolve();

                            Merge(response.AnswerRecords, remote.AnswerRecords);
                            Merge(response.AuthorityRecords, remote.AuthorityRecords);
                            Merge(response.AdditionalRecords, remote.AdditionalRecords);
                        }

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

        public MasterFile MasterFile {
            get { return masterFile; }
        }

        protected virtual void OnRequested(ClientRequest request) {
            RequestedEventHandler handlers = Requested;
            if (handlers != null) handlers(request);
        }

        protected virtual void OnResponded(ClientRequest request, Response response) {
            RespondedEventHandler handlers = Responded;
            if (handlers != null) handlers(request, response);
        }

        private ClientResponse Resolve(ClientRequest request) {
            ClientResponse response = new ClientResponse(request);

            foreach (Question question in request.Questions.ToArray()) {
                IList<IResourceRecord> answers = masterFile.Get(question);

                if (answers.Count > 0) {
                    request.Questions.Remove(question);
                    Merge(response.AnswerRecords, answers);
                }
            }

            return response;
        }

        private static void Merge<T>(IList<T> l1, IList<T> l2) {
            foreach (T obj in l2) {
                l1.Add(obj);
            }
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
