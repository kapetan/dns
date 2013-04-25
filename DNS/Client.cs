using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

using DNS.Protocol;

namespace DNS {
    public class Client {
        private const int DEFAULT_PORT = 53;
        private static readonly Random RANDOM = new Random();

        private IPEndPoint dns;
        private IRequestResolver resolver;

        private static string FormatReverseIP(IPAddress ip) {
            byte[] address = ip.GetAddressBytes();

            if (address.Length == 4) {
                return string.Join(".", address.Reverse().Select(b => b.ToString())) + ".in-addr.arpa";
            }

            byte[] nibbles = new byte[address.Length * 2];

            for (int i = 0, j = 0; i < address.Length; i++, j = 2 * i) {
                byte b = address[i];

                nibbles[j] = b.GetBitValueAt(4, 4);
                nibbles[j + 1] = b.GetBitValueAt(0, 4);
            }

            return string.Join(".", nibbles.Reverse().Select(b => b.ToString("x"))) + ".ip6.arpa";
        }

        /*private static IPEndPoint[] C(params string[] ip) {
            return ip.Select(i => new IPEndPoint(IPAddress.Parse(i), DEFAULT_PORT)).ToArray();
        }

        public static readonly IDictionary<string, IPEndPoint[]> DNS = new Dictionary<string, IPEndPoint[]>() { 
            { "Google", C("8.8.8.8", "8.8.4.4") },
            { "OpenDNS", C("208.67.222.222", "208.67.220.220") }
        };

        public static IPEndPoint Create(string name = null) {
            if (name == null) {
                name = DNS.Keys.ElementAt(RANDOM.Next(DNS.Keys.Count));
            }

            IPEndPoint[] endPoints = DNS[name];
            return endPoints[RANDOM.Next(endPoints.Length)];
        }*/

        public Client(IPEndPoint dns, IRequestResolver resolver = null) {
            this.dns = dns;
            this.resolver = resolver == null ? new UdpRequestResolver(new TcpRequestResolver()) : resolver;
        }

        public Client(IPAddress ip, int port = DEFAULT_PORT, IRequestResolver resolver = null) : 
            this(new IPEndPoint(ip, port), resolver) {}
        public Client(string ip, int port = DEFAULT_PORT, IRequestResolver resolver = null) : 
            this(IPAddress.Parse(ip), port, resolver) { }

        public ClientRequest FromArray(byte[] message) {
            Request request = Request.FromArray(message);
            return new ClientRequest(dns, request, resolver);
        }

        public ClientRequest Create(IRequest request = null) {
            return new ClientRequest(dns, request, resolver);
        }

        public IList<IPAddress> Lookup(string domain, RecordType type = RecordType.A) {
            if (type != RecordType.A && type != RecordType.AAAA) {
                throw new ArgumentException("Invalid record type " + type);
            }

            ClientResponse response = Resolve(domain, type);
            IList<IPAddress> ips = response.AnswerRecords
                .Where(r => r.Type == type)
                .Cast<IPAddressResourceRecord>()
                .Select(r => r.IPAddress)
                .ToList();

            if (ips.Count == 0) {
                throw new ResponseException(response, "No matching records");
            }

            return ips;
        }

        public string Reverse(string ip) {
            return Reverse(IPAddress.Parse(ip));
        }

        public string Reverse(IPAddress ip) {
            ClientResponse response = Resolve(FormatReverseIP(ip), RecordType.PTR);
            IResourceRecord ptr = response.AnswerRecords.FirstOrDefault(r => r.Type == RecordType.PTR);

            if (ptr == null) {
                throw new ResponseException(response, "No matching records");
            }

            return ((PointerResourceRecord) ptr).PointerDomainName.ToString();
        }

        public ClientResponse Resolve(string domain, RecordType type) {
            ClientRequest request = Create();
            Question question = new Question(new Domain(domain), type);

            request.Questions.Add(question);
            //request.AddQuestion(question);
            request.OperationCode = OperationCode.Query;
            request.RecursionDesired = true;

            return request.Resolve();
        }
    }

    public class ResponseException : Exception {
        private static string Format(IResponse response) {
            return string.Format("Invalid response received with code {0}", response.ResponseCode);
        }

        public ResponseException() {}
        public ResponseException(string message) : base(message) {}
        public ResponseException(string message, Exception e) : base(message, e) {}

        public ResponseException(IResponse response) : this(response, Format(response)) {}
        
        public ResponseException(IResponse response, Exception e) : base(Format(response), e) {
            Response = Response;
        }

        public ResponseException(IResponse response, string message) : base(message) {
            Response = Response;
        }

        public IResponse Response {
            get;
            private set;
        }
    }

    public interface IRequestResolver {
        ClientResponse Request(ClientRequest request);
    }

    public class NullRequestResolver : IRequestResolver {
        public ClientResponse Request(ClientRequest request) {
            throw new ResponseException("Request failed");
        }
    }

    public class UdpRequestResolver : IRequestResolver {
        private IRequestResolver fallback;

        public UdpRequestResolver(IRequestResolver fallback) {
            this.fallback = fallback;
        }

        public UdpRequestResolver() {
            this.fallback = new NullRequestResolver();
        }

        public ClientResponse Request(ClientRequest request) {
            UdpClient udp = new UdpClient();
            IPEndPoint dns = request.Dns;

            try {
                udp.Connect(dns);
                udp.Send(request.ToArray(), request.Size);

                byte[] buffer = udp.Receive(ref dns);
                Response response = Response.FromArray(buffer); //null;

                if (response.Truncated) {
                    return fallback.Request(request);
                }

                //try {
                //    response = Response.FromArray(buffer);
                /*} catch (ArgumentException e) {
                    throw new ResponseException("Invalid response", e);
                }*/

                /*if (response.Id != request.Id) {
                    throw new ResponseException(response, "Mismatching request/response IDs");
                }
                if (response.ResponseCode != ResponseCode.NoError) {
                    throw new ResponseException(response);
                }*/

                return new ClientResponse(request, response, buffer);
            } finally {
                udp.Close();
            }
        }
    }

    public class TcpRequestResolver : IRequestResolver {
        public ClientResponse Request(ClientRequest request) {
            TcpClient tcp = new TcpClient();

            try {
                tcp.Connect(request.Dns);

                Stream stream = tcp.GetStream();
                byte[] buffer = request.ToArray(true);

                stream.Write(buffer, 0, buffer.Length);

                buffer = new byte[2];
                Read(stream, buffer);

                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(buffer);
                }

                buffer = new byte[BitConverter.ToUInt16(buffer, 0)];
                Read(stream, buffer);

                Response response = Response.FromArray(buffer);

                return new ClientResponse(request, response, buffer);
            } finally {
                tcp.Close();
            }
        }

        private static void Read(Stream stream, byte[] buffer) {
            int length = buffer.Length;
            int offset = 0;
            int size = 0;

            while (length > 0 && (size = stream.Read(buffer, offset, length)) > 0) {
                offset += size;
                length -= size;
            }

            if (length > 0) {
                throw new IOException("Unexpected end of stream");
            }
        }
    }

    public class ClientResponse : IResponse {
        private Response response;

        public static ClientResponse FromArray(ClientRequest request, byte[] message) {
            Response response = Response.FromArray(message);
            return new ClientResponse(request, response, message);
        }

        internal ClientResponse(ClientRequest request, Response response, byte[] message) {
            Request = request;
            OriginalMessage = message;

            this.response = response;
        }

        internal ClientResponse(ClientRequest request, Response response) {
            Request = request;
            OriginalMessage = response.ToArray(false);

            this.response = response;
        }

        public byte[] OriginalMessage {
            get;
            private set;
        }

        public ClientRequest Request {
            get;
            private set;
        }

        public int Id {
            get { return response.Id; }
            set {}
        }

        public IList<IResourceRecord> AnswerRecords {
            get { return response.AnswerRecords; }
        }

        /*public void AddAnswerRecord(IResourceRecord record) {
            response.AddAnswerRecord(record);
        }*/

        public IList<IResourceRecord> AuthorityRecords {
            get { return new ReadOnlyCollection<IResourceRecord>(response.AuthorityRecords); }
        }

        /*public void AddAuthorityRecord(IResourceRecord record) {
            response.AddAuthorityRecord(record);
        }*/

        public IList<IResourceRecord> AdditionalRecords {
            get { return new ReadOnlyCollection<IResourceRecord>(response.AdditionalRecords); }
        }

        /*public void AddAdditionalRecord(IResourceRecord record) {
            response.AddAdditionalRecord(record);
        }*/

        public bool RecursionAvailable {
            get { return response.RecursionAvailable; }
            set { }
        }

        public bool AuthorativeServer {
            get { return response.AuthorativeServer; }
            set { }
        }

        public bool Truncated {
            get { return response.Truncated; }
            set { }
        }

        public OperationCode OperationCode {
            get { return response.OperationCode; }
            set { }
        }

        public ResponseCode ResponseCode {
            get { return response.ResponseCode; }
            set { }
        }

        public IList<Question> Questions {
            get { return new ReadOnlyCollection<Question>(response.Questions); }
        }

        public int Size {
            get { return response.Size; }
        }

        public byte[] ToArray(bool lengthPrefix = false) {
            return response.ToArray(lengthPrefix);
        }

        public override string ToString() {
            return response.ToString();
        }
    }

    public class ClientRequest : IRequest {
        private const int DEFAULT_PORT = 53;
        
        private IPEndPoint dns;
        private IRequestResolver resolver;
        private IRequest request;

        /*public static ClientRequest FromArray(byte[] message) {
            ClientRequest request = new ClientRequest();

            request.request = Request.FromArray(message);
            request.dns = null; //GetRandomDNS();

            return request;
        }*/

        public ClientRequest(IPEndPoint dns, IRequest request = null, IRequestResolver resolver = null) {
            this.dns = dns;
            this.request = request == null ? new Request() : new Request(request);
            this.resolver = resolver == null ? new UdpRequestResolver() : resolver;
        }

        public ClientRequest(IPAddress ip, int port = DEFAULT_PORT, IRequest request = null, IRequestResolver resolver = null) : 
            this(new IPEndPoint(ip, port), request, resolver) {}

        public ClientRequest(string ip, int port = DEFAULT_PORT, IRequest request = null, IRequestResolver resolver = null) : 
            this(IPAddress.Parse(ip), port, request, resolver) {}

        /*internal ClientRequest(Request request, IPEndPoint dns) {
            this.request = request;
            this.dns = dns;
        }*/

        public int Id {
            get { return request.Id; }
            set { request.Id = value; }
        }

        public OperationCode OperationCode {
            get { return request.OperationCode; }
            set { request.OperationCode = value; }
        }

        public bool RecursionDesired {
            get { return request.RecursionDesired; }
            set { request.RecursionDesired = value; }
        }

        /*public int QuestionCount {
            get { return request.QuestionCount; }
        }*/

        /*public void AddQuestion(Question question) {
            request.AddQuestion(question);
        }*/

        public IList<Question> Questions {
            get { return request.Questions; }
        }

        public int Size {
            get { return request.Size; }
        }

        public byte[] ToArray(bool lengthPrefix = false) {
            return request.ToArray(lengthPrefix);
        }

        public override string ToString() {
            return request.ToString();
        }

        public IPEndPoint Dns {
            get { return dns; }
            set { dns = value; }
        }

        /// <summary>
        /// Resolves this request into a response using the provided DNS information. The given
        /// request strategy is used to retrieve the response.
        /// </summary>
        /// <exception cref="ResponseException">Throw if a malformed response is received from the server</exception>
        /// <exception cref="IOException">Thrown if a IO error occurs</exception>
        /// <exception cref="SocketException">Thrown if a the reading or writing to the socket fials</exception>
        /// <returns>The response received from server</returns>
        public ClientResponse Resolve() {
            try {
                ClientResponse response = resolver.Request(this);

                if (response.Id != this.Id) {
                    throw new ResponseException(response, "Mismatching request/response IDs");
                }
                if (response.ResponseCode != ResponseCode.NoError) {
                    throw new ResponseException(response);
                }

                return response;
            } catch (ArgumentException e) {
                throw new ResponseException("Invalid response", e);
            }

            /*UdpClient udp = new UdpClient();

            try {
                udp.Connect(dns);
                udp.Send(this.ToArray(), this.Size);

                byte[] buffer = udp.Receive(ref dns);
                Response response = null;

                try {
                    response = Response.FromArray(buffer);
                } catch (ArgumentException e) {
                    throw new ResponseException("Invalid response", e);
                }

                if (response.Id != this.Id) {
                    throw new ResponseException(response, "Mismatching request/response IDs");
                }
                if (response.ResponseCode != ResponseCode.NoError) {
                    throw new ResponseException(response);
                }

                return new ClientResponse(this, response, buffer);
            } finally {
                udp.Close();
            }*/
        }
    }
}
