﻿using System;
using System.Collections.Generic;
#if (!PORTABLE)
using System.Net.Sockets;
#endif
using System.Threading.Tasks;
using System.IO;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Protocol.Utils;
using DNS.Client;
using DNS.Client.RequestResolver;

namespace DNS.Server
{
    public class DnsServer : IDisposable
    {
        private const int DEFAULT_PORT = 53;
        private const int UDP_TIMEOUT = 2000;

        public delegate void RequestedEventHandler(IRequest request);
        public delegate void RespondedEventHandler(IRequest request, IResponse response);
        public delegate void ErroredEventHandler(Exception e);

        private volatile bool run = true;
        private bool disposed = false;
        private MasterFile masterFile;
#if (!PORTABLE)
        private UdpClient udp;
#else
        private Sockets.Plugin.UdpSocketClient udp;

#endif
        private DnsClient client;

        public event RequestedEventHandler Requested;
        public event RespondedEventHandler Responded;
        public event ErroredEventHandler Errored;

        public DnsServer(IPEndPoint endServer)
        {
            this.client = new DnsClient(endServer, new UdpRequestResolver());
            this.masterFile = new MasterFile();
        }

        public DnsServer(IPAddress endServer, int port = DEFAULT_PORT) : this(new IPEndPoint(endServer, port)) { }
        public DnsServer(string endServerIp, int port = DEFAULT_PORT) : this(IPAddress.Parse(endServerIp), port) { }

        public async Task Listen(int port = DEFAULT_PORT)
        {
            await Task.Yield();

            if (run)
            {
                try
                {
#if (!PORTABLE)
                    udp = new UdpClient(port);
#else
                    udp = new Sockets.Plugin.UdpSocketClient();
                    await udp.ConnectAsync(IPAddress.Any.ToString(), port);
#endif
                }
                catch (Exception e)
                {
                    OnErrored(e);
                    return;
                }
            }
#if (!PORTABLE)
            while (run)
            {
                UdpReceiveResult result;
                try
                {
                    result = await udp.ReceiveAsync();
                }
                catch (ObjectDisposedException e) { OnErrored(e); }
                catch (SocketException e)
                {
                    OnErrored(e);
                    continue;
                }
                HandleRequest(result);
            }
#else
            if (run)
            {
                udp.MessageReceived += (s, e) =>
                {
                    HandleRequest(e);
                };
            }
#endif
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public MasterFile MasterFile
        {
            get { return masterFile; }
        }

        protected virtual void OnRequested(IRequest request)
        {
            RequestedEventHandler handlers = Requested;
            if (handlers != null) handlers(request);
        }

        protected virtual void OnResponded(IRequest request, IResponse response)
        {
            RespondedEventHandler handlers = Responded;
            if (handlers != null) handlers(request, response);
        }

        protected virtual void OnErrored(Exception e)
        {
            ErroredEventHandler handlers = Errored;
            if (handlers != null) handlers(e);
        }

        protected virtual async Task<IResponse> ResolveLocal(Request request)
        {
            Response response = Response.FromRequest(request);

            foreach (Question question in request.Questions)
            {
                IList<IResourceRecord> answers = masterFile.Get(question);

                if (answers.Count > 0)
                {
                    Merge(response.AnswerRecords, answers);
                }
                else
                {
                    return await ResolveRemote(request);
                }
            }

            return response;
        }

        protected virtual async Task<IResponse> ResolveRemote(Request request)
        {
            ClientRequest remoteRequest = client.Create(request);
            return await remoteRequest.Resolve();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                if (disposing)
                {
                    run = false;
                    udp?.Dispose();
                }
            }
        }

        
#if (!PORTABLE)
        private async void HandleRequest(UdpReceiveResult result)
#else
        private async void HandleRequest(Sockets.Plugin.Abstractions.UdpSocketMessageReceivedEventArgs result)
#endif
        {
            Request request = null;

            try
            {
#if (!PORTABLE)
                request = Request.FromArray(result.Buffer);
#else
                request = Request.FromArray(result.ByteData);
#endif
                OnRequested(request);

                IResponse response = await ResolveLocal(request);
#if (!PORTABLE)
                OnResponded(request, response);
                await udp
                    .SendAsync(response.ToArray(), response.Size, result.RemoteEndPoint)
                    .WithCancellationTimeout(UDP_TIMEOUT);
#else
                OnResponded(request, response);
                await udp
                    .SendAsync(response.ToArray());
#endif

            }
#if (!PORTABLE)
            catch (SocketException e) { OnErrored(e); }
#endif
            catch (ArgumentException e) { OnErrored(e); }
            catch (OperationCanceledException e) { OnErrored(e); }
            catch (IOException e) { OnErrored(e); }
            catch (ObjectDisposedException e) { OnErrored(e); }
            catch (ResponseException e)
            {
                IResponse response = e.Response;

                if (response == null)
                {
                    response = Response.FromRequest(request);
                }

                try
                {
#if (!PORTABLE)
                    await udp
                        .SendAsync(response.ToArray(), response.Size, result.RemoteEndPoint)
                        .WithCancellationTimeout(UDP_TIMEOUT);
#else
                    await udp
                        .SendAsync(response.ToArray());
#endif
                }
#if (!PORTABLE)
                catch (SocketException) { }
#endif
                catch (OperationCanceledException) { }
                finally { OnErrored(e); }
            }
        }

        private static void Merge<T>(IList<T> l1, IList<T> l2)
        {
            foreach (T obj in l2)
            {
                l1.Add(obj);
            }
        }
    }
}
