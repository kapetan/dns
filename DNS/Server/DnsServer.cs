using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Protocol.Utils;
using DNS.Client;
using DNS.Client.RequestResolver;

namespace DNS.Server {
    public class DnsServer: IDisposable {
        private const int DEFAULT_PORT = 53;
        private const int UDP_TIMEOUT = 2000;

        public delegate void RequestedEventHandler(IRequest request);
        public delegate void RespondedEventHandler(IRequest request, IResponse response);
        public delegate void ListeningEventHandler();
        public delegate void ErroredEventHandler(Exception e);

        private volatile bool run = true;
        private bool disposed = false;
        private MasterFile masterFile;
        private UdpClient udp;
        private DnsClient client;

        public event RequestedEventHandler Requested;
        public event RespondedEventHandler Responded;
        public event ListeningEventHandler Listening;
        public event ErroredEventHandler Errored;

        public DnsServer(IPEndPoint endServer) {
            this.client = new DnsClient(endServer, new UdpRequestResolver());
            this.masterFile = new MasterFile();
        }

        public DnsServer(IPAddress endServer, int port = DEFAULT_PORT) : this(new IPEndPoint(endServer, port)) {}
        public DnsServer(string endServerIp, int port = DEFAULT_PORT) : this(IPAddress.Parse(endServerIp), port) {}

        public async Task Listen(int port = DEFAULT_PORT) {
            await Task.Yield();

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);

            if (run) {
                try {
                    udp = new UdpClient(ip);
                } catch (SocketException e) {
                    OnErrored(e);
                    return;
                }
            }

            AsyncCallback receiveCallback = null;
            receiveCallback = result => {
                byte[] data;

                try {
                    data = udp.EndReceive(result, ref ip);
                    HandleRequest(data, ip);
                }
                catch (ObjectDisposedException) {
                    // run should already be false
                    run = false;
                }
                catch (SocketException e) {
                    OnErrored(e);
                }

                if (run) udp.BeginReceive(receiveCallback, null);
                else tcs.SetResult(null);
            };

            udp.BeginReceive(receiveCallback, null);
            OnListening();
            await tcs.Task;
        }

        public void Dispose() {
            Dispose(true);
        }

        public MasterFile MasterFile {
            get { return masterFile; }
        }

        protected virtual void OnRequested(IRequest request) {
            RequestedEventHandler handlers = Requested;
            if (handlers != null) handlers(request);
        }

        protected virtual void OnResponded(IRequest request, IResponse response) {
            RespondedEventHandler handlers = Responded;
            if (handlers != null) handlers(request, response);
        }

        protected virtual void OnListening() {
            ListeningEventHandler handlers = Listening;
            if (handlers != null) handlers();
        }

        protected virtual void OnErrored(Exception e) {
            ErroredEventHandler handlers = Errored;
            if (handlers != null) handlers(e);
        }

        protected virtual async Task<IResponse> ResolveLocal(Request request) {
            Response response = Response.FromRequest(request);

            foreach (Question question in request.Questions) {
                IList<IResourceRecord> answers = masterFile.Get(question);

                if (answers.Count > 0) {
                    Merge(response.AnswerRecords, answers);
                } else {
                    return await ResolveRemote(request);
                }
            }

            return response;
        }

        protected virtual async Task<IResponse> ResolveRemote(Request request) {
            ClientRequest remoteRequest = client.Create(request);
            return await remoteRequest.Resolve();
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposed) {
                disposed = true;

                if (disposing) {
                    run = false;
                    udp?.Dispose();
                }
            }
        }

        private async void HandleRequest(byte[] data, IPEndPoint remote) {
            Request request = null;

            try {
                request = Request.FromArray(data);
                OnRequested(request);

                IResponse response = await ResolveLocal(request);

                OnResponded(request, response);
                await udp
                    .SendAsync(response.ToArray(), response.Size, remote)
                    .WithCancellationTimeout(UDP_TIMEOUT);
            }
            catch (SocketException e) { OnErrored(e); }
            catch (ArgumentException e) { OnErrored(e); }
            catch (OperationCanceledException e) { OnErrored(e); }
            catch (IOException e) { OnErrored(e); }
            catch (ObjectDisposedException e) { OnErrored(e); }
            catch (ResponseException e) {
                IResponse response = e.Response;

                if (response == null) {
                    response = Response.FromRequest(request);
                }

                try {
                    await udp
                        .SendAsync(response.ToArray(), response.Size, remote)
                        .WithCancellationTimeout(UDP_TIMEOUT);
                }
                catch (SocketException) {}
                catch (OperationCanceledException) {}
                finally { OnErrored(e); }
            }
        }

        private static void Merge<T>(IList<T> l1, IList<T> l2) {
            foreach (T obj in l2) {
                l1.Add(obj);
            }
        }
    }
}
