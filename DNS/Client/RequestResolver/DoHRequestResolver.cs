using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DNS.Client.RequestResolver
{
    public enum DoHProtocolMode
    {
        Rfc8484Post,
        Rfc8484Get,
        CloudflareJson,
    }

    public class DoHRequestResolver : IRequestResolver
    {
        private const string mime = "application/dns-message";
        private const string json_mime = "application/dns-json";
        private static readonly HttpClient http = new HttpClient();
        private readonly Uri dns;
        private readonly DoHProtocolMode mode;

        public DoHRequestResolver(string dns, DoHProtocolMode mode = DoHProtocolMode.Rfc8484Post)
            : this(new Uri(dns), mode)
        {
        }

        public DoHRequestResolver(Uri dns, DoHProtocolMode mode = DoHProtocolMode.Rfc8484Post)
        {
            this.dns = dns;
            this.mode = mode;
            switch (dns.Scheme)
            {
                case "https":
                    // we're safe
                    break;
                case "http":
                    // not safe, warning
                    Console.Error.WriteLine("DNS over plain HTTP is unsafe");
                    break;
                default:
                    throw new NotSupportedException($"DoH can't running over {dns.Scheme} protocol");
            }
        }

        public async Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default)
        {
            // set id to 0, rfc8484 4.1
            int id = request.Id;
            request.Id = 0;

            IResponse response;
            switch (mode)
            {
                case DoHProtocolMode.Rfc8484Get:
                    {
                        UriBuilder ub = new UriBuilder(dns);
                        System.Collections.Specialized.NameValueCollection query = HttpUtility.ParseQueryString(dns.Query);

                        byte[] reqByte = request.ToArray();
                        string reqBase64 = Convert.ToBase64String(reqByte)
                                .TrimEnd('=')
                                .Replace('+', '-')
                                .Replace('/', '_');
                        query["dns"] = reqBase64;
                        ub.Query = query.ToString();
                        // GET /dns-query?dns={base64url(query)} HTTP/2
                        // Accept: application/dns-message
                        //
                        HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Get, ub.Uri);
                        reqMsg.Headers.Clear();
                        reqMsg.Headers.Add("Accept", mime);

                        HttpResponseMessage hrm = await http.SendAsync(reqMsg, cancellationToken);
                        if (!hrm.IsSuccessStatusCode)
                        {
                            throw new WebException($"DoH server return code {hrm.StatusCode}");
                        }

                        byte[] respByte = await hrm.Content.ReadAsByteArrayAsync();
                        response = Response.FromArray(respByte);

                        break;
                    }
                case DoHProtocolMode.Rfc8484Post:
                    {
                        byte[] reqByte = request.ToArray();

                        // POST /dns-query HTTP/2
                        // Accept: application/dns-message
                        // Content-Type: application/dns-message
                        // Content-Length: {query.length}
                        //
                        // {query}

                        HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Post, dns);
                        reqMsg.Headers.Clear();
                        reqMsg.Headers.Add("Accept", mime);
                        reqMsg.Content = new ByteArrayContent(reqByte);

                        HttpResponseMessage hrm = await http.SendAsync(reqMsg, cancellationToken);
                        if (!hrm.IsSuccessStatusCode)
                        {
                            throw new WebException($"DoH server return code {hrm.StatusCode}");
                        }

                        byte[] respByte = await hrm.Content.ReadAsByteArrayAsync();
                        response = Response.FromArray(respByte);
                        break;
                    }
                case DoHProtocolMode.CloudflareJson:
                    {
                        if (request.Questions.Count != 1)
                        {
                            throw new NotSupportedException("DoH JSON API can only has 1 question per request");
                        }
                        Question q = request.Questions[0];

                        UriBuilder ub = new UriBuilder(dns);
                        System.Collections.Specialized.NameValueCollection query = HttpUtility.ParseQueryString(dns.Query);


                        query["name"] = q.Name.ToString();
                        query["type"] = q.Type.ToString();

                        ub.Query = query.ToString();
                        // GET /dns-query?name={qname}&type={qtype} HTTP/2
                        // Accept: application/dns-json
                        //
                        HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Get, ub.Uri);
                        reqMsg.Headers.Clear();
                        reqMsg.Headers.Add("Accept", json_mime);
                        HttpResponseMessage hrm = await http.SendAsync(reqMsg, cancellationToken);
                        if (!hrm.IsSuccessStatusCode)
                        {
                            throw new WebException($"DoH server return code {hrm.StatusCode}");
                        }
                        string responseText = await hrm.Content.ReadAsStringAsync();

                        // for server return questions in object not in array
                        JObject r = JObject.Parse(responseText);
                        var responseQ = r.GetValue("Question", StringComparison.OrdinalIgnoreCase);
                        if (responseQ is JObject)
                        {
                            r["Question"] = new JArray(responseQ);
                        }
                        DoHCloudflareJsonResponse responseJson = r.ToObject<DoHCloudflareJsonResponse>();
                        response = responseJson.GetResponse();
                        break;
                    }
                default:
                    throw new NotSupportedException();
            }

            // recover original id
            request.Id = id;
            response.Id = id;
            return new ClientResponse(request, response);
        }
    }

    internal class DoHCloudflareJsonResponse
    {
        public int Status;
        public bool TC;
        public bool RD;
        public bool RA;
        public bool AD;
        public bool CD;
        public List<DoHCloudflareJsonResponseQuestion> Question;
        public List<DoHCloudflareJsonResponseAnswer> Answer;
        public List<DoHCloudflareJsonResponseAnswer> Authority;
        public List<DoHCloudflareJsonResponseAnswer> Additional;

        public Response GetResponse()
        {
            Response resp = new Response
            {
                Truncated = TC,
                // RecursiveDesired = RD,
                RecursionAvailable = RA,
                AuthenticData = AD,
                CheckingDisabled = CD,
            };

            foreach (DoHCloudflareJsonResponseQuestion q in Question)
            {
                resp.Questions.Add(q.GetQuestion());
            }
            if (Answer != null)
                foreach (DoHCloudflareJsonResponseAnswer a in Answer)
                {
                    resp.AnswerRecords.Add(a.GetResourceRecord());
                }
            if (Authority != null)
                foreach (DoHCloudflareJsonResponseAnswer a in Authority)
                {
                    resp.AuthorityRecords.Add(a.GetResourceRecord());
                }
            if (Additional != null)

                foreach (DoHCloudflareJsonResponseAnswer a in Additional)
                {
                    resp.AdditionalRecords.Add(a.GetResourceRecord());
                }
            return resp;
        }
    }

    internal class DoHCloudflareJsonResponseQuestion
    {
        public string name;
        public int type;
        public Question GetQuestion()
        {
            return new Question(Domain.FromString(name), (RecordType)Enum.ToObject(typeof(RecordType), type));
        }
    }

    internal class DoHCloudflareJsonResponseAnswer
    {
        public string name;
        public int type;
        public int TTL;
        public string data;
        public IResourceRecord GetResourceRecord()
        {
            Domain domain = Domain.FromString(name);
            TimeSpan ttl = TimeSpan.FromSeconds(TTL);
            // not registered rr
            if (!Enum.IsDefined(typeof(RecordType), type))
            {
                byte[] dataByte = Enumerable.Range(0, data.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(data.Substring(x, 2), 16))
                     .ToArray();
                return new ResourceRecord(
                    domain,
                    dataByte,
                    (RecordType)type,
                    RecordClass.IN,
                    ttl
                    );
            }
            RecordType r = (RecordType)Enum.ToObject(typeof(RecordType), type);
            // well known rr
            switch (r)
            {
                case RecordType.CNAME:
                    return new CanonicalNameResourceRecord(domain, Domain.FromString(data), ttl);
                case RecordType.A:
                case RecordType.AAAA:
                    return new IPAddressResourceRecord(domain, IPAddress.Parse(data), ttl);
                case RecordType.NS:
                    return new NameServerResourceRecord(domain, Domain.FromString(data), ttl);
                case RecordType.PTR:
                    return new PointerResourceRecord(IPAddress.Parse(name), Domain.FromString(data), ttl);
                case RecordType.MX:
                    string[] mx = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (mx.Length != 2)
                    {
                        throw new FormatException();
                    }

                    return new MailExchangeResourceRecord(domain, int.Parse(mx[0]), Domain.FromString(mx[1]), ttl);
                case RecordType.SRV:
                    string[] srv = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (srv.Length != 4)
                    {
                        throw new FormatException();
                    }

                    return new ServiceResourceRecord(
                        domain,
                        ushort.Parse(srv[0]),
                        ushort.Parse(srv[1]),
                        ushort.Parse(srv[2]),
                        Domain.FromString(srv[3]),
                        ttl);
                case RecordType.SOA:
                    string[] soa = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (soa.Length != 7)
                    {
                        throw new FormatException();
                    }

                    return new StartOfAuthorityResourceRecord(
                        domain,
                        Domain.FromString(soa[0]),
                        Domain.FromString(soa[1]),
                        long.Parse(soa[2]),
                        TimeSpan.FromSeconds(long.Parse(soa[3])),
                        TimeSpan.FromSeconds(long.Parse(soa[4])),
                        TimeSpan.FromSeconds(long.Parse(soa[5])),
                        TimeSpan.FromSeconds(long.Parse(soa[6])),
                        ttl);
                case RecordType.TXT:
                    return new TextResourceRecord(domain, CharacterString.FromString(data), ttl);
            }
            // registered but has no corresponding rr class
            return new ResourceRecord(
                    Domain.FromString(name),
                    Encoding.ASCII.GetBytes(data),
                    (RecordType)Enum.ToObject(typeof(RecordType), type),
                    RecordClass.IN,
                    TimeSpan.FromSeconds(TTL)
                    );
        }
    }
}
