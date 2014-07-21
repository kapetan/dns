using Kapetan.Dns.Interface;
using Kapetan.Dns.Model;

namespace Kapetan.Dns.Resolver
{
    public class NullRequestResolver : IRequestResolver
    {
        public ClientResponse Request(ClientRequest request)
        {
            throw new ResponseException("Request failed");
        }
    }
}
