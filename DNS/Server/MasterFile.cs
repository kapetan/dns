using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Client.RequestResolver;

namespace DNS.Server {
    public class MasterFile : IRequestResolver {
        protected static readonly TimeSpan DEFAULT_TTL = new TimeSpan(0);

        protected static bool Matches(Domain domain, Domain entry) {
            string[] labels = entry.ToString().Split('.');
            string[] patterns = new string[labels.Length];

            for (int i = 0; i < labels.Length; i++) {
                string label = labels[i];
                patterns[i] = label == "*" ? "(\\w+)" : Regex.Escape(label);
            }

            Regex re = new Regex("^" + string.Join("\\.", patterns) + "$", RegexOptions.IgnoreCase);
            return re.IsMatch(domain.ToString());
        }

        protected static void Merge<T>(IList<T> l1, IList<T> l2) {
            foreach (T obj in l2) {
                l1.Add(obj);
            }
        }

        protected IList<IResourceRecord> entries = new List<IResourceRecord>();
        protected TimeSpan ttl = DEFAULT_TTL;

        public MasterFile(TimeSpan ttl) {
            this.ttl = ttl;
        }

        public MasterFile() {}

        public void Add(IResourceRecord entry) {
            entries.Add(entry);
        }

        public void AddIPAddressResourceRecord(string domain, string ip) {
            AddIPAddressResourceRecord(new Domain(domain), IPAddress.Parse(ip));
        }

        public void AddIPAddressResourceRecord(Domain domain, IPAddress ip) {
            Add(new IPAddressResourceRecord(domain, ip, ttl));
        }

        public void AddNameServerResourceRecord(string domain, string nsDomain) {
            AddNameServerResourceRecord(new Domain(domain), new Domain(nsDomain));
        }

        public void AddNameServerResourceRecord(Domain domain, Domain nsDomain) {
            Add(new NameServerResourceRecord(domain, nsDomain, ttl));
        }

        public void AddCanonicalNameResourceRecord(string domain, string cname) {
            AddCanonicalNameResourceRecord(new Domain(domain), new Domain(cname));
        }

        public void AddCanonicalNameResourceRecord(Domain domain, Domain cname) {
            Add(new CanonicalNameResourceRecord(domain, cname, ttl));
        }

        public void AddPointerResourceRecord(string ip, string pointer) {
            AddPointerResourceRecord(IPAddress.Parse(ip), new Domain(pointer));
        }

        public void AddPointerResourceRecord(IPAddress ip, Domain pointer) {
            Add(new PointerResourceRecord(ip, pointer, ttl));
        }

        public void AddMailExchangeResourceRecord(string domain, int preference, string exchange) {
            AddMailExchangeResourceRecord(new Domain(domain), preference, new Domain(exchange));
        }

        public void AddMailExchangeResourceRecord(Domain domain, int preference, Domain exchange) {
            Add(new MailExchangeResourceRecord(domain, preference, exchange));
        }

        public void AddTextResourceRecord(string domain, string attributeName, string attributeValue) {
            Add(new TextResourceRecord(new Domain(domain), attributeName, attributeValue, ttl));
        }

        public void AddServiceResourceRecord(Domain domain, ushort priority, ushort weight, ushort port, Domain target) {
            Add(new ServiceResourceRecord(domain, priority, weight, port, target, ttl));
        }

        public void AddServiceResourceRecord(string domain, ushort priority, ushort weight, ushort port, string target) {
            AddServiceResourceRecord(new Domain(domain), priority, weight, port, new Domain(target));
        }

        public Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default(CancellationToken)) {
            IResponse response = Response.FromRequest(request);

            foreach (Question question in request.Questions) {
                IList<IResourceRecord> answers = Get(question);

                if (answers.Count > 0) {
                    Merge(response.AnswerRecords, answers);
                } else {
                    response.ResponseCode = ResponseCode.NameError;
                }
            }

            return Task.FromResult(response);
        }

        protected IList<IResourceRecord> Get(Domain domain, RecordType type) {
            return entries.Where(e => Matches(domain, e.Name) && (e.Type == type || type == RecordType.ANY)).ToList();
        }

        protected IList<IResourceRecord> Get(Question question) {
            return Get(question.Name, question.Type);
        }
    }
}
