using System.Threading.Tasks;

namespace DNS.Client.RequestResolver {
    public class NullRequestResolver : IRequestResolver {
        public Task<ClientResponse> Request(ClientRequest request) {
            throw new ResponseException("Request failed");
        }
    }
}
