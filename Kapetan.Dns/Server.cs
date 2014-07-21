using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kapetan.Dns.Model;
using System.Net.Sockets;
using System.Net;
using Kapetan.Dns.Resolver;
using System.Threading;
using System.Collections.Concurrent;
using Kapetan.Dns.Interface;

namespace Kapetan.Dns
{
    public class Server
    {
        private const int DEFAULT_PORT = 53;
        private const int UDP_TIMEOUT = 2000;
        private const int UDP_LIMIT = 512;

        public delegate void RequestedEventHandler(IRequest request);
        public delegate void RespondedEventHandler(IRequest request, IResponse response);

        private volatile bool run = true;

        private MasterFile masterFile;

        private UdpClient udpClient;
        private EventEmitter emitter;
        private Client client;

        public event RequestedEventHandler Requested;
        public event RespondedEventHandler Responded;

        public Server(IPEndPoint endServer)
        {
            this.emitter = new EventEmitter();
            this.client = new Client(endServer, new UdpRequestResolver());
            this.masterFile = new MasterFile();
        }

        public Server(IPAddress endServer, int port = DEFAULT_PORT) : this(new IPEndPoint(endServer, port)) { }
        public Server(string endServerIp, int port = DEFAULT_PORT) : this(IPAddress.Parse(endServerIp), port) { }

        public void Listen(int port = DEFAULT_PORT)
        {
            this.udpClient = new UdpClient(port);

            var local = new IPEndPoint(IPAddress.Any, port);

            this.emitter.Run();
            this.udpClient.Client.SendTimeout = UDP_TIMEOUT;

            while (this.run)
            {
                byte[] clientMessage = null;

                try
                {
                    clientMessage = udpClient.Receive(ref local);
                }
                catch (SocketException)
                {
                    continue;
                }

                var task = new Thread(() =>
                {
                    Request request = null;

                    try
                    {
                        request = Request.FromArray(clientMessage);
                        emitter.Schedule(() => OnRequested(request));

                        var response = ResolveLocal(request);

                        emitter.Schedule(() => Responded(request, response));
                        this.udpClient.Send(response.ToArray(), response.Size, local);
                    }
                    catch (SocketException) { }
                    catch (ArgumentException) { }
                    catch (ResponseException e)
                    {
                        IResponse response = e.Response;

                        if (response == null)
                        {
                            response = Response.FromRequest(request);
                        }

                        this.udpClient.Send(response.ToArray(), response.Size, local);
                    }
                });

                task.Start();
            }
        }

        public void Close()
        {
            if (udpClient != null)
            {
                run = false;

                emitter.Stop();
                udpClient.Close();
            }
        }

        public MasterFile MasterFile
        {
            get { return masterFile; }
        }

        protected virtual void OnRequested(IRequest request)
        {
            var handlers = Requested;
            if (handlers != null)
            {
                handlers(request);
            }
        }

        protected virtual void OnResponded(IRequest request, IResponse response)
        {
            var handlers = Responded;
            if (handlers != null)
            {
                handlers(request, response);
            }
        }

        protected virtual IResponse ResolveLocal(Request request)
        {
            var response = Response.FromRequest(request);

            foreach (var question in request.Questions)
            {
                IList<IResourceRecord> answers = masterFile.Get(question);

                if (answers.Count > 0)
                {
                    Merge(response.AnswerRecords, answers);
                }
                else
                {
                    return ResolveRemote(request);
                }
            }

            return response;
        }

        protected virtual IResponse ResolveRemote(Request request)
        {
            var remoteRequest = client.Create(request);
            return remoteRequest.Resolve();
        }

        private static void Merge<T>(IList<T> l1, IList<T> l2)
        {
            foreach (T obj in l2)
            {
                l1.Add(obj);
            }
        }
    }

    internal class EventEmitter
    {
        public delegate void Emit();

        private CancellationTokenSource tokenSource;
        private BlockingCollection<Emit> queue;

        public void Schedule(Emit emit)
        {
            if (queue != null)
            {
                queue.Add(emit);
            }
        }

        public void Run()
        {
            this.tokenSource = new CancellationTokenSource();
            this.queue = new BlockingCollection<Emit>();

            (new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        var emit = queue.Take(tokenSource.Token);
                        emit();
                    }
                }
                catch (OperationCanceledException) { }
            })).Start();
        }

        public void Stop()
        {
            if (tokenSource != null)
            {
                tokenSource.Cancel();
            }
        }
    }
}