using System.Threading.Tasks;
using DNS.Protocol;

namespace DNS.Client.RequestResolver {
    public class NullRequestResolver : IRequestResolver {
        public Task<IResponse> Request(IRequest request) {
            throw new ResponseException("Request failed");
        }
    }
}
