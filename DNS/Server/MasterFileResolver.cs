using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DNS.Server {
    public abstract class MasterFileResolver : IRequestResolver {

        public interface IEntry {
            IResourceRecord Record { get; }
            bool IsMatch(Question question);
        }

        protected IList<IEntry> Entries = new List<IEntry>();

        public Task<IResponse> Resolve(IRequest request) {
            IResponse response = Response.FromRequest(request);

            foreach (var question in request.Questions) {
                var matches = Entries.Where(e => e.IsMatch(question)).ToList();

                foreach (var entry in matches) {
                    response.AnswerRecords.Add(entry.Record);
                }
            }

            if (response.AnswerRecords.Count == 0) {
                response.ResponseCode = ResponseCode.NameError;
            }

            return Task.FromResult(response);
        }
    }
}
