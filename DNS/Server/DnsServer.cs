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
    public class DnsServer : IDisposable {
        private const int DEFAULT_PORT = 53;
        private const int UDP_TIMEOUT = 2000;

        public delegate void RequestedEventHandler(IRequest request);
        public delegate void RespondedEventHandler(IRequest request, IResponse response);
        public delegate void ListeningEventHandler();
        public delegate void ErroredEventHandler(Exception e);

        public event RequestedEventHandler Requested;
        public event RespondedEventHandler Responded;
        public event ListeningEventHandler Listening;
        public event ErroredEventHandler Errored;

        private bool run = true;
        private bool disposed = false;
        private UdpClient udp;
        private IRequestResolver resolver;

        public DnsServer(MasterFile masterFile, IPEndPoint endServer) :
            this(new FallbackRequestResolver(masterFile, new UdpRequestResolver(endServer))) {}

        public DnsServer(MasterFile masterFile, IPAddress endServer, int port = DEFAULT_PORT) :
            this(masterFile, new IPEndPoint(endServer, port)) {}

        public DnsServer(MasterFile masterFile, string endServer, int port = DEFAULT_PORT) :
            this(masterFile, IPAddress.Parse(endServer), port) {}

        public DnsServer(IPEndPoint endServer) :
            this(new UdpRequestResolver(endServer)) {}

        public DnsServer(IPAddress endServer, int port = DEFAULT_PORT) :
            this(new IPEndPoint(endServer, port)) {}

        public DnsServer(string endServer, int port = DEFAULT_PORT) :
            this(IPAddress.Parse(endServer), port) {}

        public DnsServer(IRequestResolver resolver) {
            this.resolver = resolver;
        }

        public Task Listen(int port = DEFAULT_PORT, IPAddress ip = null) {
            return Listen(new IPEndPoint(ip ?? IPAddress.Any, port));
        }

        public async Task Listen(IPEndPoint endpoint) {
            await Task.Yield();

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            if (run) {
                try {
                    udp = new UdpClient(endpoint);
                } catch (SocketException e) {
                    OnErrored(e);
                    return;
                }
            }

            AsyncCallback receiveCallback = null;
            receiveCallback = result => {
                byte[] data;

                try {
                    IPEndPoint remote = new IPEndPoint(0, 0);
                    data = udp.EndReceive(result, ref remote);
                    HandleRequest(data, remote);
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
                request.RemoteAddress = remote.Address;

                OnRequested(request);

                IResponse response = await resolver.Resolve(request);

                OnResponded(request, response);
                await udp
                    .SendAsync(response.ToArray(), response.Size, remote)
                    .WithCancellationTimeout(UDP_TIMEOUT);
            }
            catch (SocketException e) { OnErrored(e); }
            catch (ArgumentException e) { OnErrored(e); }
            catch (IndexOutOfRangeException e) { OnErrored(e); }
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

        private class FallbackRequestResolver : IRequestResolver {
            private IRequestResolver[] resolvers;

            public FallbackRequestResolver(params IRequestResolver[] resolvers) {
                this.resolvers = resolvers;
            }

            public async Task<IResponse> Resolve(IRequest request) {
                IResponse response = null;

                foreach (IRequestResolver resolver in resolvers) {
                    response = await resolver.Resolve(request);
                    if (response.AnswerRecords.Count > 0) break;
                }

                return response;
            }
        }
    }
}
