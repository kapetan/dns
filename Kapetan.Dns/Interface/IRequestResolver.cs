using Kapetan.Dns.Model;

namespace Kapetan.Dns.Interface
{
    public interface IRequestResolver
    {
        ClientResponse Request(ClientRequest request);
    }
}
