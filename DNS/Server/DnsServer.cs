﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using DNS.Protocol;
using DNS.Protocol.Utils;
using DNS.Client;
using DNS.Client.RequestResolver;

namespace DNS.Server {
    public class DnsServer : IDisposable {
        private const int SIO_UDP_CONNRESET = unchecked((int) 0x9800000C);
        private const int DEFAULT_PORT = 53;
        private const int UDP_TIMEOUT = 2000;

        public event EventHandler<RequestedEventArgs> Requested;
        public event EventHandler<RespondedEventArgs> Responded;
        public event EventHandler<EventArgs> Listening;
        public event EventHandler<ErroredEventArgs> Errored;

        private bool run = true;
        private bool disposed = false;
        private UdpClient udp;
        private IRequestResolver resolver;

        public DnsServer(IRequestResolver resolver, IPEndPoint endServer) :
            this(new FallbackRequestResolver(resolver, new UdpRequestResolver(endServer))) {}

        public DnsServer(IRequestResolver resolver, IPAddress endServer, int port = DEFAULT_PORT) :
            this(resolver, new IPEndPoint(endServer, port)) {}

        public DnsServer(IRequestResolver resolver, string endServer, int port = DEFAULT_PORT) :
            this(resolver, IPAddress.Parse(endServer), port) {}

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

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                        udp.Client.IOControl(SIO_UDP_CONNRESET, new byte[4], new byte[4]);
                    }
                } catch (SocketException e) {
                    OnError(e);
                    return;
                }
            }

            void ReceiveCallback(IAsyncResult result) {
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
                    OnError(e);
                }

                if (run) udp.BeginReceive(ReceiveCallback, null);
                else tcs.SetResult(null);
            }

            udp.BeginReceive(ReceiveCallback, null);
            OnEvent(Listening, EventArgs.Empty);
            await tcs.Task.ConfigureAwait(false);
        }

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void OnEvent<T>(EventHandler<T> handler, T args) {
            if (handler != null) handler(this, args);
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

        private void OnError(Exception e) {
            OnEvent(Errored, new ErroredEventArgs(e));
        }

        private async void HandleRequest(byte[] data, IPEndPoint remote) {
            Request request = null;

            try {
                request = Request.FromArray(data);
                OnEvent(Requested, new RequestedEventArgs(request, data, remote));

                IResponse response = await resolver.Resolve(request).ConfigureAwait(false);

                OnEvent(Responded, new RespondedEventArgs(request, response, data, remote));
                await udp
                    .SendAsync(response.ToArray(), response.Size, remote)
                    .WithCancellationTimeout(TimeSpan.FromMilliseconds(UDP_TIMEOUT)).ConfigureAwait(false);
            }
            catch (SocketException e) { OnError(e); }
            catch (ArgumentException e) { OnError(e); }
            catch (IndexOutOfRangeException e) { OnError(e); }
            catch (OperationCanceledException e) { OnError(e); }
            catch (IOException e) { OnError(e); }
            catch (ObjectDisposedException e) { OnError(e); }
            catch (ResponseException e) {
                IResponse response = e.Response;

                if (response == null) {
                    response = Response.FromRequest(request);
                }

                try {
                    await udp
                        .SendAsync(response.ToArray(), response.Size, remote)
                        .WithCancellationTimeout(TimeSpan.FromMilliseconds(UDP_TIMEOUT)).ConfigureAwait(false);
                }
                catch (SocketException) {}
                catch (OperationCanceledException) {}
                finally { OnError(e); }
            }
        }

        public class RequestedEventArgs : EventArgs {
            public RequestedEventArgs(IRequest request, byte[] data, IPEndPoint remote) {
                Request = request;
                Data = data;
                Remote = remote;
            }

            public IRequest Request { get; }
            public byte[] Data { get; }
            public IPEndPoint Remote { get; }
        }

        public class RespondedEventArgs : EventArgs {
            public RespondedEventArgs(IRequest request, IResponse response, byte[] data, IPEndPoint remote) {
                Request = request;
                Response = response;
                Data = data;
                Remote = remote;
            }

            public IRequest Request { get; }
            public IResponse Response { get; }
            public byte[] Data { get; }
            public IPEndPoint Remote { get; }
        }

        public class ErroredEventArgs : EventArgs {
            public ErroredEventArgs(Exception e) {
                Exception = e;
            }

            public Exception Exception { get; }
        }

        private class FallbackRequestResolver : IRequestResolver {
            private IRequestResolver[] resolvers;

            public FallbackRequestResolver(params IRequestResolver[] resolvers) {
                this.resolvers = resolvers;
            }

            public async Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default(CancellationToken)) {
                IResponse response = null;

                foreach (IRequestResolver resolver in resolvers) {
                    response = await resolver.Resolve(request, cancellationToken).ConfigureAwait(false);
                    if (response.AnswerRecords.Count > 0) break;
                }

                return response;
            }
        }
    }
}
